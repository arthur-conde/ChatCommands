using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using Wetstone.Hooks;

namespace ChatCommands.Abstractions
{
    public interface ICommandHandler
    {
        bool HandleCommands(VChatEvent ev, ConfigFile config);
    }
}
