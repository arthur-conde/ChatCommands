using System;
using System.Collections.Generic;
using System.Text;
using ProjectM;

namespace ChatCommands.Models
{
    public record SimpleItem(int Id, string Name)
    {
        public PrefabGUID ToGuid() => new (Id);
    }
}
