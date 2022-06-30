using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using ChatCommands.Abstractions;
using ChatCommands.Attributes;
using ChatCommands.Services;
using ChatCommands.Utils;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SimpleInjector;
using Wetstone.API;
using Wetstone.Hooks;

namespace ChatCommands
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("xyz.molenzwiebel.wetstone")]
    [Reloadable]
    public class Plugin : BasePlugin
    {
        internal const string KitsConfigPath = "BepInEx/config/ChatCommands/kits.json";
        internal const string ConfigPath = "BepInEx/config/ChatCommands";
        private Harmony harmony;

        private LegacyCommandHandler cmd;
        private ConfigEntry<string> Prefix;
        private ConfigEntry<string> DisabledCommands;
        private ConfigEntry<int> WaypointLimit;

        private Container container;

        private void InitConfig()
        {
            Prefix = Config.Bind("Config", "Prefix", "?", "The prefix used for chat commands.");
            DisabledCommands = Config.Bind("Config", "Disabled Commands", "", "Enter command names to disable them. Seperated by commas. Ex.: health,speed");
            WaypointLimit = Config.Bind("Config", "Waypoint Limit", 3, "Sets a waypoint limit per user.");

            try
            {
                if (!Directory.Exists(ConfigPath))
                    Directory.CreateDirectory(ConfigPath);

                if (!File.Exists(KitsConfigPath))
                {
                    using var stream = File.Create(KitsConfigPath);
                }

                if (!File.Exists("BepInEx/config/ChatCommands/permissions.json"))
                {
                    using var stream = File.Create("BepInEx/config/ChatCommands/permissions.json");
                }
            }
            catch (Exception e)
            {
                Log.LogWarning("Unable to create json");
                Log.LogError(e);
            }
        }

        public override void Load()
        {
            var serviceCollection = new Container();

            // Handle AutoInject Attribute
            foreach (var serviceGroup in Assembly.GetExecutingAssembly().GetTypes()
                         .Select(t => new { Type = t, Metadata = t.GetCustomAttribute<AutoInjectAttribute>(false) })
                         .Where(e => e.Metadata != null)
                         .GroupBy(e => e.Metadata.ServiceType)
                         .Select(e => new { Type = e.Key, ConcreteImplementations = e.Select(e => e.Type).ToList()}))
            {
                Log.LogInfo($"Service: {serviceGroup.Type.Name}");
                if (serviceGroup.ConcreteImplementations.Count == 1)
                {
                    var concreteType = serviceGroup.ConcreteImplementations.First();
                    Log.LogInfo($"\tImplementation: {concreteType.Name}");
                    serviceCollection.Register(serviceGroup.Type, concreteType);
                }
                else
                {
                    foreach(var concreteType in serviceGroup.ConcreteImplementations)
                        serviceCollection.Collection.Append(serviceGroup.Type, concreteType);
                }
            }

            serviceCollection.RegisterInstance(Log);
            serviceCollection.Register<IExpressionCache<ProjectM.UnitStats>, ExpressionCache<ProjectM.UnitStats>>(Lifestyle.Singleton);

            foreach (var chatCommand in Assembly.GetExecutingAssembly().GetTypes()
                         .Where(t => t.GetCustomAttribute(typeof(ChatCommandAttribute), false) is { }))
            {
                Log.LogInfo($"Found Command Handler: {chatCommand.Name}");
                serviceCollection.Collection.Append(typeof(IChatCommand), chatCommand);
            }

            var commandHandlerOptions = new CommandHandlerOptions
            {
                Prefix= Prefix.Value, 
                DisabledCommands = DisabledCommands.Value,
            };
            serviceCollection.RegisterInstance<ICommandHandlerOptions>(commandHandlerOptions);

            cmd = new LegacyCommandHandler(Prefix.Value, DisabledCommands.Value);
            serviceCollection.RegisterInstance(cmd);

            Log.LogInfo("Building container...");
            container = serviceCollection;
            Log.LogInfo("Built container.");

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            // Create config directory and json
            var jsonConfigService = serviceCollection.GetInstance<IJsonConfigService>();
            jsonConfigService.Initialize();

            // Do magic
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            // Hook events
            Chat.OnChatMessage += HandleChatMessage;
        }

        public override bool Unload()
        {
            Config.Clear();
            Chat.OnChatMessage -= HandleChatMessage;

            return true;
        }

        private void HandleChatMessage(VChatEvent ev)
        {
            if (container.GetInstance<ICommandHandler>() is { } newCommandHandler &&
                newCommandHandler.HandleCommands(ev, Log, Config))
                return;
            Log.LogInfo("Unable to retrieve command handler. Falling back to legacy.");
            cmd.HandleCommands(ev, Log, Config);
        }
    }
}
