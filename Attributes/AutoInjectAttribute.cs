using System;

namespace ChatCommands.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoInjectAttribute: Attribute
    {
        public Type ServiceType { get; set; }

        public AutoInjectAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }
    }
}
