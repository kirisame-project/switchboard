using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using Switchboard.Common;

namespace Switchboard.Controllers.WebSocketsNg
{
    [Component(ComponentLifestyle.Singleton)]
    public class MemoryStreamPool : IObjectOwner<MemoryStream>
    {
        private readonly int? _maximumRetained;
        private readonly ConcurrentBag<WeakReference<MemoryStream>> _pool;

        public MemoryStreamPool(int? maximumRetained = null)
        {
            _maximumRetained = maximumRetained;
            _pool = new ConcurrentBag<WeakReference<MemoryStream>>();
        }

        public void Return(MemoryStream stream)
        {
            if (_maximumRetained != null && _maximumRetained < _pool.Count) return;

            stream.SetLength(0);
            _pool.Add(new WeakReference<MemoryStream>(stream));

            Debug.Assert(stream.Position == 0);
        }

        public MemoryStream Get()
        {
            while (_pool.TryTake(out var reference))
                if (reference.TryGetTarget(out var stream))
                    return stream;
            return new MemoryStream();
        }
    }
}