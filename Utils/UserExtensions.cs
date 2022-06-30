using System;
using System.Collections.Generic;
using System.Text;
using Wetstone.API;

namespace ChatCommands.Utils
{
    public static class UserExtensions
    {
        /// <summary>
        /// Send a System message, with color!
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public static void SendMessage(this ProjectM.Network.User user, string message, string color = "ff0000ff")
            => user.SendSystemMessage($"<color=#{color}>{message}</color>");
    }
}
