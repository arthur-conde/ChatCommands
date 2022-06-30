using ChatCommands.Abstractions;
using ChatCommands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChatCommands.Services
{
    [AutoInject(typeof(IChatCommandCache))]
    public class ChatCommandCache : IChatCommandCache
    {
        private readonly Lazy<IReadOnlyDictionary<Type, ChatCommandAttribute>> _cache;

        public ChatCommandCache(IEnumerable<IChatCommand> commands)
        {
            var commandTypes = commands.Select(v => v.GetType()).ToList();

            _cache = new Lazy<IReadOnlyDictionary<Type, ChatCommandAttribute>>(() =>
            {
                var cache = new Dictionary<Type, ChatCommandAttribute>();
                foreach (var commandType in commandTypes)
                {
                    if (commandType.GetCustomAttribute<ChatCommandAttribute>() is { } cca)
                        cache.Add(commandType, cca);
                }

                return cache;
            });
        }

        public ChatCommandAttribute GetMetadata(Type t)
        {
            return _cache.Value.ContainsKey(t) ? _cache.Value[t] : null;
        }
    }
}
