using System;
using System.Collections.Generic;
using System.Text;

namespace ChatCommands.Abstractions
{
    public interface ICommandHandlerOptions
    {
        string Prefix { get; }
        string DisabledCommands { get; }
    }
}
