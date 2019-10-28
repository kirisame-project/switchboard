using System;

namespace Switchboard.Common
{
    public class DependsAttribute : Attribute
    {
        public DependsAttribute(Type type, ComponentLifestyle lifestyle = ComponentLifestyle.Singleton)
        {
            Lifestyle = lifestyle;
            Type = type;
        }

        public ComponentLifestyle Lifestyle { get; }

        public Type Type { get; }
    }
}