using System;

namespace Switchboard.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        public ComponentAttribute(ComponentLifestyle lifestyle)
        {
            Lifestyle = lifestyle;
        }

        public Type Implements { get; set; } = null;

        public ComponentLifestyle Lifestyle { get; set; }
    }
}