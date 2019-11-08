using System;

namespace Switchboard.Controllers.WebSocketsNg
{
    internal class RentedObjectHolder<T> : IDisposable
    {
        private readonly IObjectOwner<T> _owner;

        public RentedObjectHolder(T stream, IObjectOwner<T> owner)
        {
            Object = stream;
            _owner = owner;
        }

        public T Object { get; }

        public void Dispose()
        {
            _owner.Return(Object);
        }
    }
}