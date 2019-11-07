using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Controllers.WebSocketized;
using Switchboard.Controllers.WebSocketized.Contracts;
using Switchboard.Services.Lambda;

namespace Switchboard.Controllers.WebSocketsNg
{
    public class WebSocketSession : IWebSocketSession
    {
        private readonly WebSocketShim _socket;

        public WebSocketSession(WebSocket socket, MemoryStreamPool memoryStreamPool)
        {
            _socket = new WebSocketShim(socket, memoryStreamPool);
            SessionId = Guid.NewGuid();
        }

        public Guid SessionId { get; }

        public void Dispose()
        {
            _socket?.Dispose();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Handshake(cancellationToken);
                SessionActive = true;

                while (!cancellationToken.IsCancellationRequested)
                    if (!await TryHandleMessage())
                    {
                        await _socket.EnsureClosedAsync(WebSocketCloseStatus.NormalClosure, "Normal closure",
                            cancellationToken);
                        return;
                    }

                await _socket.EnsureClosedAsync(WebSocketCloseStatus.InternalServerError, "Session cancelled",
                    CancellationToken.None);
                throw new OperationCanceledException(cancellationToken);
            }
            catch (WebSocketException)
            {
                await _socket.EnsureClosedAsync(WebSocketCloseStatus.ProtocolError, "WebSocket operation failed",
                    cancellationToken);
                throw;
            }
            catch
            {
                await _socket.EnsureClosedAsync(WebSocketCloseStatus.InternalServerError, "Internal server error",
                    cancellationToken);
                throw;
            }
            finally
            {
                SessionActive = false;
            }
        }

        public Task SendTaskUpdateAsync(LambdaTask task, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public bool SessionActive { get; private set; }

        private Task<bool> TryHandleMessage()
        {
            return Task.FromResult(true);
        }

        private async Task Handshake(CancellationToken cancellationToken)
        {
            using var streamHolder = await _socket.ReceiveMessageAsync(cancellationToken);
            var stream = streamHolder.Object;

            stream.Seek(0, SeekOrigin.Begin);
            var str = await new StreamReader(stream).ReadToEndAsync();

            stream.Seek(0, SeekOrigin.Begin);
            var _ = await JsonSerializer.DeserializeAsync<ClientHandshake>(stream,
                cancellationToken: cancellationToken);

            var serverHandshake = new ServerHandshake(new ServerHandshake.ServerHandshakePayload
            {
                ServerId = Guid.Empty,
                ServerName = Environment.MachineName,
                SessionId = SessionId
            });
            await _socket.SendObjectAsync(serverHandshake, cancellationToken);
        }
    }
}
