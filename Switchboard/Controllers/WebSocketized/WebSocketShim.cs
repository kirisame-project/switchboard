using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;

namespace Switchboard.Controllers.WebSocketized
{
    public class WebSocketShim
    {
        private readonly ObjectPool<byte[]> _bufferPool;

        private readonly WebSocket _socket;

        public WebSocketShim(WebSocket socket, ObjectPool<byte[]> bufferPool)
        {
            _socket = socket;
            _bufferPool = bufferPool;
        }

        public WebSocketState State => _socket.State;

        public async Task EnsureClosedAsync(WebSocketCloseStatus code, string reason,
            CancellationToken cancellationToken)
        {
            if (_socket.State == WebSocketState.Open)
                await _socket.CloseAsync(code, reason, cancellationToken);
        }

        public async Task<WebSocketMessageType> ReceiveMessageAsync(Stream output, CancellationToken cancellationToken)
        {
            var buffer = _bufferPool.Get();
            try
            {
                var offset = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    var segment = new ArraySegment<byte>(buffer, offset, buffer.Length - offset);
                    var result = await _socket.ReceiveAsync(segment, cancellationToken);

                    offset += result.Count;
                    if (offset >= buffer.Length)
                        throw new ArgumentOutOfRangeException(nameof(offset), offset, "Buffer overflow");

                    if (!result.EndOfMessage) continue;

                    output.SetLength(offset);
                    output.Seek(0, SeekOrigin.Begin);
                    await output.WriteAsync(buffer, 0, offset, cancellationToken);
                    return result.MessageType;
                }

                throw new TaskCanceledException();
            }
            finally
            {
                _bufferPool.Return(buffer);
            }
        }

        public async Task<T> ReceiveObjectAsync<T>(CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();
            await ReceiveMessageAsync(stream, cancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
        }

        public async Task SendObjectAsync<T>(T obj, CancellationToken cancellationToken)
        {
            var buffer = _bufferPool.Get();
            try
            {
                await using var stream = new MemoryStream(buffer, true);
                await JsonSerializer.SerializeAsync(stream, obj, cancellationToken: cancellationToken);
                await _socket.SendAsync(new ArraySegment<byte>(buffer, 0, (int) stream.Position),
                    WebSocketMessageType.Text, true, cancellationToken);
            }
            finally
            {
                _bufferPool.Return(buffer);
            }
        }
    }
}
