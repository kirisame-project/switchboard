using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using Switchboard.Controllers.WebSocketized.Contracts;

namespace Switchboard.Controllers.WebSocketized
{
    public class WebSocketSession
    {
        private readonly WebSocketSessionConfiguration _config;
        private readonly Guid _sessionId;

        private readonly WebSocketShim _socket;

        public WebSocketSession(WebSocket socket, ObjectPool<byte[]> bufferPool, WebSocketSessionConfiguration config)
        {
            _config = config;
            _socket = new WebSocketShim(socket, bufferPool);
            _sessionId = Guid.NewGuid();
        }

        public event Action OnClose;

        private async Task Handshake(CancellationToken cancellationToken)
        {
            var _ = await _socket.ReceiveObjectAsync<ClientHandshake>(cancellationToken);
            await _socket.SendObjectAsync(new ServerHandshake(new ServerHandshake.ServerHandshakePayload
            {
                ServerId = Guid.Empty,
                ServerName = Environment.MachineName,
                SessionId = _sessionId
            }), cancellationToken);
        }

        private async Task ReceiveForeverAsync(CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();
            while (!cancellationToken.IsCancellationRequested)
            {
                var _ = await _socket.ReceiveMessageAsync(stream, cancellationToken);
                stream.Seek(0, SeekOrigin.Begin);
                var message = await JsonSerializer.DeserializeAsync<Message>(stream,
                    cancellationToken: cancellationToken);
                switch (message.OperationCode)
                {
                    case OperationCode.Close:
                        await _socket.SendObjectAsync(new ConnectionClosed
                        {
                            Error = null, Reason = "Bye"
                        }, cancellationToken);
                        return;
                    case OperationCode.Handshake:
                        throw new WebSocketOperationException(
                            "Unexpected repeated handshake",
                            WebSocketCloseStatus.ProtocolError);
                    case OperationCode.Heartbeat:
                        await _socket.SendObjectAsync(new Heartbeat(), cancellationToken);
                        // TODO: reset heartbeat timer
                        break;
                    default:
                        throw new WebSocketOperationException(
                            "Operation not implemented",
                            WebSocketCloseStatus.ProtocolError);
                }
            }
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                await Handshake(cancellationToken);
                await ReceiveForeverAsync(cancellationToken);
                await _socket.EnsureClosedAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Normal closure",
                    cancellationToken);
            }
            catch (JsonException)
            {
                await _socket.SendObjectAsync(new ConnectionClosed
                {
                    Error = "JSON deserialization failed",
                    Reason = "Unexpected exception"
                }, cancellationToken);
                await _socket.EnsureClosedAsync(
                    WebSocketCloseStatus.ProtocolError,
                    "JSON deserialization failed",
                    cancellationToken);
            }
            catch (WebSocketOperationException e)
            {
                await _socket.SendObjectAsync(new ConnectionClosed
                {
                    Error = e.Message,
                    Reason = "WebSocket operation failed"
                }, cancellationToken);
                await _socket.EnsureClosedAsync(e.Code, e.Message, cancellationToken);
            }
            catch (Exception)
            {
                await _socket.EnsureClosedAsync(
                    WebSocketCloseStatus.InternalServerError,
                    "Abnormal closure",
                    cancellationToken);
            }
            finally
            {
                OnClose?.Invoke();
            }
        }
    }

    public class WebSocketSessionConfiguration
    {
        public int HeartbeatInterval { get; set; }
    }
}
