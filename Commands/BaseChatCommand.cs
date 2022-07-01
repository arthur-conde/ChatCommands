using BepInEx.Logging;
using ChatCommands.Abstractions;
using ChatCommands.Models;

namespace ChatCommands.Commands;

public abstract class BaseChatCommand : IChatCommand
{
    protected ManualLogSource Logger { get; }

    protected BaseChatCommand(ManualLogSource logger)
    {
        Logger = logger;
    }

    public abstract CommandResult Handle(CommandContext ctx);
}