using System;
using System.Collections.Generic;
using System.Text;

namespace ChatCommands.Abstractions
{
    public interface ICommandHandlerOptions
    {
        string Prefix { get; set; }
        string DisabledCommands { get; set; }
    }
}
