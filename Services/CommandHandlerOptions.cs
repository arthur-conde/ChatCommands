using System;
using System.Collections.Generic;
using System.Text;
using ChatCommands.Abstractions;

namespace ChatCommands.Services
{
    public class CommandHandlerOptions : ICommandHandlerOptions
    {
        public string Prefix { get; set; }

        public string DisabledCommands { get; set; }
    }
}
