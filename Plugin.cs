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
using ChatCommands.Models;
using SimpleInjector;
using SimpleInjector.Lifestyles;
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
        }

        public override void Load()
        {
            // Create config
            InitConfig();

            var serviceCollection = new Container();
            serviceCollection.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            // Handle AutoInject Attribute
            foreach (var serviceGroup in Assembly.GetExecutingAssembly().GetTypes()
                         .Select(t => new { Type = t, Metadata = t.GetCustomAttribute<AutoInjectAttribute>(false) })
                         .Where(e => e.Metadata != null)
                         .GroupBy(e => e.Metadata.ServiceType)
                         .Select(e => new { Type = e.Key, ConcreteImplementations = e.ToList()}))
            {
                Log.LogInfo($"Service: {serviceGroup.Type.Name}");
                if (serviceGroup.ConcreteImplementations.Count == 1)
                {
                    var concreteImplementation = serviceGroup.ConcreteImplementations.First();
                    Log.LogInfo($"\tImplementation: {concreteImplementation.Type} - Lifetime: {concreteImplementation.Metadata.Lifetime}");

                    var lifetime = concreteImplementation.Metadata.Lifetime switch
                    {
                        ServiceLifetime.Singleton => Lifestyle.Singleton,
                        ServiceLifetime.Transient => Lifestyle.Transient,
                        _ => Lifestyle.Scoped,
                    };
                    serviceCollection.Register(serviceGroup.Type, concreteImplementation.Type, lifetime);
                }
                else
                {
                    foreach (var concreteImplementation in serviceGroup.ConcreteImplementations)
                    {
                        var lifetime = concreteImplementation.Metadata.Lifetime switch
                        {
                            ServiceLifetime.Singleton => Lifestyle.Singleton,
                            ServiceLifetime.Transient => Lifestyle.Transient,
                            _ => Lifestyle.Scoped,
                        };
                        serviceCollection.Collection.Append(serviceGroup.Type, concreteImplementation.Type, lifetime);
                    }
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
                Prefix = Prefix.Value,
                DisabledCommands = DisabledCommands.Value,
            };
            Log.LogInfo("Registering Command Options...");
            serviceCollection.RegisterInstance<ICommandHandlerOptions>(commandHandlerOptions);

            cmd = new LegacyCommandHandler(Prefix.Value, DisabledCommands.Value);
            serviceCollection.RegisterInstance(cmd);

            Log.LogInfo("Building container...");
            container = serviceCollection;
            Log.LogInfo("Built container.");

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            

            // Create JSON
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

        private async void HandleChatMessage(VChatEvent ev)
        {
            await using var scope = AsyncScopedLifestyle.BeginScope(container);
            if (container.GetInstance<ICommandHandler>() is { } newCommandHandler &&
                newCommandHandler.HandleCommands(ev, Config))
                return;
            Log.LogInfo("Unable to retrieve command handler. Falling back to legacy.");
            cmd.HandleCommands(ev, Log, Config);
        }
    }
}
