using System;
using System.Buffers;
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

        public async Task EnsureClosedAsync(WebSocketCloseStatus code, string reason, CancellationToken token)
        {
            if (_socket.State == WebSocketState.Open)
                await _socket.CloseAsync(code, reason, token);
        }

        private async Task<MemoryStream> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            var stream = _streamPool.Get();
            var buffer = ArrayPool<byte>.Shared.Rent(ChunkBufferSize);
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await _socket.ReceiveAsync(buffer, cancellationToken);
                    await stream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, result.Count), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure,
                            "Closure requested by the client", cancellationToken);
                        throw new WebSocketException(WebSocketError.ConnectionClosedPrematurely);
                    }

                    if (result.EndOfMessage)
                        return stream;
                }

                throw new OperationCanceledException(cancellationToken);
            }
            catch
            {
                _streamPool.Return(stream);
                throw;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public async Task<RentedObjectHolder<MemoryStream>> ReceiveStreamAsync(CancellationToken cancellationToken)
        {
            return new RentedObjectHolder<MemoryStream>(await ReceiveMessageAsync(cancellationToken), _streamPool);
        }

        public async Task<T> ReceiveObjectAsync<T>(CancellationToken cancellationToken)
        {
            await using var stream = await ReceiveMessageAsync(cancellationToken);
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
            }
            finally
            {
                _streamPool.Return(stream);
            }
        }

        public async Task<DeserializationContext> ReceiveObjectAsync(CancellationToken cancellationToken)
        {
            return new DeserializationContext(await ReceiveMessageAsync(cancellationToken), _streamPool);
        }

        public async Task SendObjectAsync<T>(T obj, CancellationToken cancellationToken)
        {
            var stream = _streamPool.Get();
            try
            {
                await JsonSerializer.SerializeAsync(stream, obj, cancellationToken: cancellationToken);
                stream.SetLength(stream.Position);

                var buffer = new byte[ChunkBufferSize];

                stream.Seek(0, SeekOrigin.Begin);
                while (stream.Length - stream.Position > ChunkBufferSize)
                {
                    await stream.ReadAsync(buffer, 0, ChunkBufferSize, cancellationToken);
                    await _socket.SendAsync(buffer, WebSocketMessageType.Text, false, cancellationToken);
                }

                var count = await stream.ReadAsync(buffer, 0, ChunkBufferSize, cancellationToken);
                var segment = new ArraySegment<byte>(buffer, 0, count);
                await _socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
            }
            finally
            {
                _streamPool.Return(stream);
            }
        }
    }
}
