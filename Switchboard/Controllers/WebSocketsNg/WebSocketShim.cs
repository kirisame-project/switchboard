using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Controllers.WebSocketsNg
{
    internal class WebSocketShim : IDisposable
    {
        private const int ChunkBufferSize = 1024 * 64; // 1KB * 64

        private readonly WebSocket _socket;

        private readonly MemoryStreamPool _streamPool;

        public WebSocketShim(WebSocket socket, MemoryStreamPool streamPool)
        {
            _socket = socket;
            _streamPool = streamPool;
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }

        public async Task<RentedObjectHolder<MemoryStream>> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            var stream = _streamPool.Get();
            try
            {
                var buffer = new byte[ChunkBufferSize];
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await _socket.ReceiveAsync(buffer, cancellationToken);
                    await stream.WriteAsync(buffer, cancellationToken);

                    if (!result.EndOfMessage) continue;

                    return new RentedObjectHolder<MemoryStream>(stream, _streamPool);
                }

                throw new OperationCanceledException(cancellationToken);
            }
            catch
            {
                _streamPool.Return(stream);
                throw;
            }
        }

        public async Task SendObjectAsync<T>(T obj, CancellationToken cancellationToken)
        {
            var stream = _streamPool.Get();
            try
            {
                await JsonSerializer.SerializeAsync(stream, obj, cancellationToken: cancellationToken);

                var buffer = new byte[ChunkBufferSize];

                var offset = 0;
                for (; offset + ChunkBufferSize < stream.Length; offset += ChunkBufferSize)
                {
                    await stream.ReadAsync(buffer, offset, ChunkBufferSize, cancellationToken);
                    await _socket.SendAsync(buffer, WebSocketMessageType.Text, false, cancellationToken);
                }

                await stream.ReadAsync(buffer, offset, (int) (stream.Length - offset), cancellationToken);
                await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
            }
            finally
            {
                _streamPool.Return(stream);
            }
        }
    }
}