using ChatCommands.Models;
using ChatCommands.Services;

namespace ChatCommands.Abstractions
{
    public interface IChatCommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        CommandResult Handle(CommandContext ctx);
    }
}
