using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using Unity.Entities;
using Wetstone.API;
using Wetstone.Hooks;

namespace ChatCommands.Models;

public class CommandContext
{
    public string Prefix { get; set; }
    public VChatEvent Event { get; set; }
    public ManualLogSource Log { get; set; }
    public IReadOnlyCollection<string> Args { get; set; }
    public ConfigFile Config { get; set; }
    public EntityManager EntityManager { get; set; }

    public string[] DisabledCommands;

    public CommandContext(string prefix, VChatEvent ev, ManualLogSource log, ConfigFile config, IReadOnlyCollection<string> args, string disabledCommands)
    {
        this.Prefix = prefix;
        this.Event = ev;
        this.Log = log;
        this.Args = args;
        this.Config = config;

        EntityManager = VWorld.Server.EntityManager;
        DisabledCommands = disabledCommands.Split(',');
    }
}