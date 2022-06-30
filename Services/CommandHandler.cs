using BepInEx.Configuration;
using BepInEx.Logging;
using ChatCommands.Abstractions;
using ChatCommands.Models;
using Il2CppSystem.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Wetstone.API;
using Wetstone.Hooks;

namespace ChatCommands.Services
{
    public class CommandHandler : ICommandHandler
    {
        private ICommandHandlerOptions Options { get; }
        private IChatCommandCache ChatCommandCache { get; }
        private IReadOnlyList<IChatCommand> ChatCommands { get; }

        public string Prefix => Options.Prefix;
        public string DisabledCommands => Options.DisabledCommands;

        public Dictionary<string, bool> Permissions { get; set; }

        private const string CommandSplitter = "(?<=\")[^\"]*(?=\")|[^\" ]+"; // Regex: (?<=")[^"]*(?=")|[^" ]+
        private readonly Regex _commandSplitterRegex;

        public CommandHandler(ICommandHandlerOptions options, IChatCommandCache chatCommandCache, IEnumerable<IChatCommand> chatCommands)
        {
            Options = options;
            ChatCommandCache = chatCommandCache;
            ChatCommands = chatCommands.ToList();

            _commandSplitterRegex = new Regex(CommandSplitter);

            LoadPermissions();
        }

        public bool HandleCommands(VChatEvent ev, ManualLogSource log, ConfigFile config)
        {
            if (!ev.Message.StartsWith(Prefix) || !VWorld.IsServer) return false;

            var message = ev.Message;
            if (_commandSplitterRegex.Match(message, 1) is not { Success: true } match)
            {
                log.LogInfo($"Failed to match regex to input: `{message}`");
                return false;
            }

            var command = match.Groups[0].Value;
            IReadOnlyCollection<string> args = Array.Empty<string>();
            if (match.Groups.Count > 1 && match.Groups is IEnumerable<Group> groups)
                args = groups.Skip(1).Select(g => g.Value).ToArray();

            var commandContext = new CommandContext(Prefix, ev, log, config, args, DisabledCommands);

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
                if (!handler.Handle(commandContext)) continue;

                log.LogInfo($"[CommandHandler] {ev.User.CharacterName} used command: {command.ToLower()}");
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
