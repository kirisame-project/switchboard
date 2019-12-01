using System;
using Microsoft.Extensions.DependencyInjection;

namespace AtomicAkarin.Shirakami.Reflections
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentAttribute : Attribute
    {
        public ComponentAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public Type ServiceType { get; set; }

        public ServiceLifetime Lifetime { get; set; }
    }
}