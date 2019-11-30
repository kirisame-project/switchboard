using System;

namespace AtomicAkarin.Shirakami.Reflections
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireExternal : Attribute
    {
        public RequireExternal(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}