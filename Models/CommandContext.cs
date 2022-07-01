using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Unity.Entities;
using Wetstone.API;
using Wetstone.Hooks;

namespace ChatCommands.Models;

public class CommandContext
{
    public string Prefix { get; set; }
    public VChatEvent Event { get; set; }
    public IReadOnlyList<string> OriginalArgs { get; set; }
    public Queue<string> Args { get; set; }
    public ConfigFile Config { get; set; }
    public EntityManager EntityManager { get; set; }

    public ProjectM.Network.User EventUser => Event?.User;
    public Unity.Entities.Entity UserEntity => Event.SenderUserEntity;
    public Unity.Entities.Entity CharacterEntity => Event.SenderCharacterEntity;

    public string[] DisabledCommands;

    public CommandContext(string prefix, VChatEvent ev, ConfigFile config, IEnumerable<string> args, string disabledCommands)
    {
        this.Prefix = prefix;
        this.Event = ev;
        OriginalArgs = args.ToList();
        this.Args = new Queue<string>(OriginalArgs);
        this.Config = config;

        EntityManager = VWorld.Server.EntityManager;
        DisabledCommands = disabledCommands.Split(',');
    }

    public string GetArgumentsForLog()
    {
        return $"{{ Original: [{string.Join(", ", OriginalArgs)}], Current: [{string.Join(", ", Args)}] }}";
    }

}