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
        bool Handle(CommandContext ctx);
    }
}
