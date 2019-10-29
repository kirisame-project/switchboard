using System;

namespace Switchboard.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DependsSingletonAttribute : Attribute
    {
        public DependsSingletonAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}