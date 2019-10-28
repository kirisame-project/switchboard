using System;

namespace Switchboard.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        public ComponentAttribute(ComponentLifestyle lifestyle = ComponentLifestyle.Singleton)
        {
            Lifestyle = lifestyle;
        }

        public ComponentLifestyle Lifestyle { get; }
    }
}