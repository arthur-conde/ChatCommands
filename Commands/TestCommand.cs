using ChatCommands.Abstractions;
using ChatCommands.Attributes;
using ChatCommands.Models;
using ChatCommands.Utils;
using Wetstone.API;

namespace ChatCommands.Commands
{
    [ChatCommand("debug", Usage = "debug prop <value|reset>", Description = "Fiddles with things")]
    public class TestCommand : IChatCommand
    {
        public bool Handle(CommandContext ctx)
        {
            ctx.Event.User.SendSystemMessage("Hello!");

            return true;
        }
    }
}
