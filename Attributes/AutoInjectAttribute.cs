using System;
using ChatCommands.Models;

namespace ChatCommands.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoInjectAttribute: Attribute
    {
        public Type ServiceType { get; set; }

        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;

        public AutoInjectAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }
    }
}
