using BepInEx.Configuration;
using BepInEx.Logging;
using ChatCommands.Abstractions;
using ChatCommands.Models;
using Il2CppSystem.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ChatCommands.Attributes;
using ChatCommands.Utils;
using Wetstone.API;
using Wetstone.Hooks;

namespace ChatCommands.Services
{
    [AutoInject(typeof(ICommandHandler))]
    public class CommandHandler : ICommandHandler
    {
        private ManualLogSource Logger { get; }
        private ICommandHandlerOptions Options { get; }
        private IChatCommandCache ChatCommandCache { get; }
        private IReadOnlyList<IChatCommand> ChatCommands { get; }

        public string Prefix => Options.Prefix;
        public string DisabledCommands => Options.DisabledCommands;

        public Dictionary<string, bool> Permissions { get; set; }

        public CommandHandler(ManualLogSource logger, ICommandHandlerOptions options, IChatCommandCache chatCommandCache, IEnumerable<IChatCommand> chatCommands)
        {
            Logger = logger;
            Options = options;
            ChatCommandCache = chatCommandCache;
            ChatCommands = chatCommands.ToList();

            LoadPermissions();
        }

        public bool HandleCommands(VChatEvent ev, ConfigFile config)
        {
            if (!ev.Message.StartsWith(Prefix) || !VWorld.IsServer) return false;

            var message = ev.Message;
            var parts = message.ParseArguments(' ', '"', '\'').ToArray();
            if (parts.Length <= 0)
            {
                Logger.LogInfo($"Failed to match regex to input: `{message}`");
                return false;
            }

            var command = parts.First().Substring(1);
            IReadOnlyList<string> args = Array.Empty<string>();
            if (parts.Length > 1)
                args = parts.Skip(1).ToList();

            Logger.LogInfo($"Attempting to call command {command} with arguments [{string.Join(", ", args)}]");

            var commandContext = new CommandContext(Prefix, ev, config, args, DisabledCommands);

            foreach (var handler in ChatCommands)
            {
                ev.Cancel();
                var commandType = handler.GetType();
                if (DisabledCommands.Split(',').Any(x => string.Equals(x, command, StringComparison.OrdinalIgnoreCase))) continue;
                if (ChatCommandCache.GetMetadata(commandType) is not {} metadata) continue;
                if (!metadata.Aliases.Any(x => string.Equals(x, command, StringComparison.OrdinalIgnoreCase))) continue;
                Permissions.TryGetValue(command, out var isAdminOnly);
                if (IsNotAdmin(commandType, ev, isAdminOnly))
                {
                    ev.User.SendSystemMessage($"You do not have the required permissions to use that.");
                    return true;
                }

                switch (handler.Handle(commandContext))
                {
                    case CommandResult.MissingArguments:
                        ev.User.SendMessage($"Missing command parameters. Check {Options.Prefix}help for more information..");
                        break;
                    case CommandResult.InvalidArguments:
                        ev.User.SendMessage($"Invalid command parameters. Check {Options.Prefix}help for more information..");
                        break;
                    case CommandResult.Unhandled:
                        continue;
                }

                Logger.LogInfo($"[CommandHandler] {ev.User.CharacterName} used command: {command.ToLower()}");
                return true;
            }

            SavePermissions();

            return false;
        }

        private bool IsNotAdmin(Type type, VChatEvent ev, bool isAdminOnly)
        {
            return isAdminOnly && !ev.User.IsAdmin;
        }

        private void LoadPermissions()
        {
            if (!File.Exists("BepInEx/config/ChatCommands/permissions.json"))
            {
                using var stream = File.Create("BepInEx/config/ChatCommands/permissions.json");
            }

            var json = File.ReadAllText("BepInEx/config/ChatCommands/permissions.json");
            try
            {
                Permissions = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
            }
            catch
            {
                Permissions = new Dictionary<string, bool>();
            }
        }

        public void SavePermissions()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                IncludeFields = true
            };
            File.WriteAllText("BepInEx/config/ChatCommands/permissions.json", JsonSerializer.Serialize(Permissions, options));
        }
    }
}
