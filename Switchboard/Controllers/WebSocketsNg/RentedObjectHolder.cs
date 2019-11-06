using System;

namespace Switchboard.Controllers.WebSocketsNg
{
    internal class RentedObjectHolder<T> : IDisposable
    {
        private readonly IObjectOwner<T> _owner;

        public RentedObjectHolder(T obj, IObjectOwner<T> owner)
        {
            Object = obj;
            _owner = owner;
        }

        public T Object { get; }

        public void Dispose()
        {
            _owner.Return(Object);
        }
    }
}