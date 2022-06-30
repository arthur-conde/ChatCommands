using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatCommands.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ChatCommandAttribute : Attribute
    {
        public List<string> Aliases;

        public string Name { get; set; }
        public string Usage { get; set; }
        public string Description { get; set; }
        public bool AdminOnly { get; set; }

        public ChatCommandAttribute(string name, string usage = "", string description = "None", bool adminOnly = false, params string[] aliases)
        {
            Name = name;
            Usage = usage;
            Description = description;
            AdminOnly = adminOnly;

            Aliases = new List<string>()
            {
                Name
            };

            if (aliases?.Any() == true)
                Aliases.AddRange(aliases);
        }
    }
}
