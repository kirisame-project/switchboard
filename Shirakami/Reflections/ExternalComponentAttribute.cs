using System;
using Microsoft.Extensions.DependencyInjection;

namespace AtomicAkarin.Shirakami.Reflections
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ExternalComponentAttribute : Attribute
    {
        public ExternalComponentAttribute(Type type, ServiceLifetime lifetime)
        {
            Type = type;
            Lifetime = lifetime;
        }

        public ServiceLifetime Lifetime { get; }

        public Type Type { get; }
    }
}