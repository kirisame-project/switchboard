using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Switchboard.Common;

namespace Switchboard.Controllers.WebSocketsX.Facilities.Buffers
{
    [Component(ComponentLifestyle.Singleton)]
    public class MemoryStreamPool : IObjectOwner<MemoryStream>
    {
        private readonly int? _maximumRetained;

        private readonly IProducerConsumerCollection<WeakReference<MemoryStream>> _pool;

        private long _referenceMissCount;

        private long _streamClosedCount;

        public MemoryStreamPool(int? maximumRetained = null)
        {
            _maximumRetained = maximumRetained;
            _pool = new ConcurrentBag<WeakReference<MemoryStream>>();
        }

        public void Return(MemoryStream stream)
        {
            if (_maximumRetained != null && _maximumRetained < _pool.Count) return;
            var result = _pool.TryAdd(new WeakReference<MemoryStream>(stream));
            Debug.Assert(result);
        }

        public MemoryStream Get()
        {
            while (_pool.TryTake(out var reference))
                if (reference.TryGetTarget(out var stream))
                    if (stream.CanRead && stream.CanWrite)
                    {
                        stream.SetLength(0);
                        return stream;
                    }
                    else
                    {
                        Interlocked.Increment(ref _streamClosedCount);
                    }
                else
                    Interlocked.Increment(ref _referenceMissCount);

            return new MemoryStream();
        }
    }
}