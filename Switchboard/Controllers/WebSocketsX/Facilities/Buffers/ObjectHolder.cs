using System;

namespace Switchboard.Controllers.WebSocketsX.Facilities.Buffers
{
    internal class ObjectHolder<T> : IDisposable
    {
        private readonly IObjectOwner<T> _owner;

        public ObjectHolder(T obj, IObjectOwner<T> owner)
        {
            _owner = owner;
            Obj = obj;
        }

        public T Obj { get; }

        public void Dispose()
        {
            _owner.Return(Obj);
        }
    }
}