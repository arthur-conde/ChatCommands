using System;
using System.Collections.Generic;
using System.Text;
using ChatCommands.Attributes;

namespace ChatCommands.Abstractions
{
    public interface IChatCommandCache
    {
        ChatCommandAttribute GetMetadata(Type t);

        ChatCommandAttribute GetMetadata<T>() where T : IChatCommand => GetMetadata(typeof(T));
    }
}
