using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace AtomicAkarin.Shirakami.Reflections
{
    public class ComponentLoader
    {
        private readonly ICollection<Type> _components;
        private readonly IServiceCollection _services;

        public ComponentLoader(IServiceCollection services)
        {
            _services = services;
            _components = new HashSet<Type>();
        }

        public void AddFromAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ComponentAttribute>();
                if (attribute == null)
                    continue;

                AddType(type, attribute);
            }
        }

        private void AddType(Type type, ComponentAttribute attribute)
        {
            _components.Add(type);

            // resolve external dependencies
            var externals = type.GetCustomAttributes<ExternalComponentAttribute>();
            foreach (var ext in externals)
                AddExactTypeLifetime(ext.Type, ext.Type, ext.Lifetime);

            // resolve constructor dependencies
            foreach (var ctor in type.GetConstructors())
            foreach (var ctorParam in ctor.GetParameters())
            {
                var paramType = ctorParam.ParameterType;
                var paramTypeAttr = paramType.GetCustomAttribute<ComponentAttribute>();
                if (paramTypeAttr == null || _components.Contains(paramType))
                    continue;

                AddType(paramType, paramTypeAttr);
            }

            var serviceType = attribute.ServiceType ?? type;
            AddExactTypeLifetime(serviceType, type, attribute.Lifetime);
        }

        private void AddExactTypeLifetime(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            _services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
        }
    }
}