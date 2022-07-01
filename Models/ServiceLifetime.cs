using System;
using System.Collections.Generic;
using System.Text;

namespace ChatCommands.Models
{
    public enum ServiceLifetime
    {
        Transient,
        Scoped,
        Singleton
    }
}
