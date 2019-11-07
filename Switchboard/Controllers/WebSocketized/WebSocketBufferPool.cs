using System;
using System.Collections.Concurrent;

namespace Switchboard.Controllers.WebSocketized
{
    [Obsolete]
    public class WebSocketBufferPool
    {
        private readonly ConcurrentBag<byte[]> _buffers;

        private readonly int _maximumRetained;
        private readonly int _size;

        public WebSocketBufferPool(int size, int maximumRetained)
        {
            _size = size;
            _maximumRetained = maximumRetained;
            _buffers = new ConcurrentBag<byte[]>();
        }

        public byte[] Get()
        {
            return _buffers.TryTake(out var instance) ? instance : new byte[_size];
        }

        public void Reduce(int count)
        {
            for (var i = 0; i < count; i++)
                if (!_buffers.TryTake(out _))
                    break;
        }

        public void Return(byte[] instance)
        {
            if (_buffers.Count <= _maximumRetained)
                _buffers.Add(instance);
        }
    }
}
