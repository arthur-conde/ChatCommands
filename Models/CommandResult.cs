using System;
using System.Collections.Generic;
using System.Text;

namespace ChatCommands.Models
{
    public enum CommandResult
    {
        MissingArguments = -2,
        InvalidArguments = -1,
        Unhandled = 0,
        Success
    }
}
