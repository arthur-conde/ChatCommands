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
                if (!File.Exists("BepInEx/config/ChatCommands/kits.json"))
                {
                    if (!Directory.Exists("BepInEx/config/ChatCommands"))
                        Directory.CreateDirectory("BepInEx/config/ChatCommands");
                    using var stream = File.Create("BepInEx/config/ChatCommands/kits.json");
                }

                if (!File.Exists("BepInEx/config/ChatCommands/permissions.json"))
                {
                    if (!Directory.Exists("BepInEx/config/ChatCommands"))
                        Directory.CreateDirectory("BepInEx/config/ChatCommands");
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
            InitConfig();
            var commandHandlerOptions = new CommandHandlerOptions
            {
                Prefix= Prefix.Value, 
                DisabledCommands = DisabledCommands.Value,
            };
            cmd = new LegacyCommandHandler(Prefix.Value, DisabledCommands.Value);
            Chat.OnChatMessage += HandleChatMessage;
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            var serviceCollection = new Container();
            serviceCollection.RegisterInstance(Log);
            serviceCollection.RegisterInstance(cmd);
            serviceCollection.RegisterInstance<ICommandHandlerOptions>(commandHandlerOptions);
            serviceCollection.Register<IChatCommandCache,ChatCommandCache>();
            serviceCollection.Register<ICommandHandler, CommandHandler>();
            serviceCollection.Register<IExpressionCache<ProjectM.UnitStats>, ExpressionCache<ProjectM.UnitStats>>(Lifestyle.Singleton);

            foreach (var chatCommand in Assembly.GetExecutingAssembly().GetTypes()
                         .Where(t => t.GetCustomAttribute(typeof(ChatCommandAttribute), false) is { }))
            {
                Log.LogInfo($"Found Command Handler: {chatCommand.Name}");
                serviceCollection.Collection.Append(typeof(IChatCommand), chatCommand);
            }

            Log.LogInfo("Building container...");
            // services = serviceCollection.BuildServiceProvider();
            container = serviceCollection;
            Log.LogInfo("Built container.");

            Log.LogInfo("Testing container...");

            var commandHandler = container.GetInstance<ICommandHandler>();
            if (commandHandler == null)
                Log.LogInfo("Failed to retrieve ICommandHandler from DI container");
            else
                Log.LogInfo("Successfully retrieved ICommandHandler from DI container");
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
