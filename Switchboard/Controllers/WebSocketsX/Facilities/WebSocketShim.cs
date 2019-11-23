using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;

namespace Switchboard.Controllers.WebSocketsX.Facilities
{
    internal class WebSocketShim : IDisposable
    {
        private const int ChunkBufferSize = 1024 * 4; // 1KB * 4
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;

        private readonly WebSocket _socket;

        public WebSocketShim(WebSocket socket, RecyclableMemoryStreamManager memoryStreamManager)
        {
            _socket = socket;
            _memoryStreamManager = memoryStreamManager;
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
            var stream = _memoryStreamManager.GetStream();
            var buffer = new byte[ChunkBufferSize];

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

        public async Task<MemoryStream> ReceiveStreamAsync(CancellationToken cancellationToken)
        {
            return await ReceiveMessageAsync(cancellationToken);
        }

        public async Task<T> ReceiveObjectAsync<T>(CancellationToken cancellationToken)
        {
            await using var stream = await ReceiveMessageAsync(cancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
        }

        public async Task SendObjectAsync<T>(T obj, CancellationToken cancellationToken)
        {
            await using var stream = _memoryStreamManager.GetStream();
            await JsonSerializer.SerializeAsync(stream, obj, cancellationToken: cancellationToken);

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
    }
}