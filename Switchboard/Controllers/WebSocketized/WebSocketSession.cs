using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Controllers.WebSocketized.Contracts;
using Switchboard.Services.Lambda;

namespace Switchboard.Controllers.WebSocketized
{
    public class WebSocketSession : IDisposable, IWebSocketSession
    {
        private readonly WebSocketSessionConfiguration _config;

        private bool _isHandshakeAccepted;

        private readonly WebSocketShim _socket;

        public WebSocketSession(WebSocket socket, WebSocketBufferPool bufferPool, WebSocketSessionConfiguration config)
        {
            _socket = new WebSocketShim(socket, bufferPool);
            _config = config;
            SessionId = Guid.NewGuid();
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }

        public Guid SessionId { get; }

        public async Task RunAsync(CancellationToken cancellationToken)
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

        public bool SessionActive => _socket.State == WebSocketState.Open && _isHandshakeAccepted;

        public async Task SendTaskUpdateAsync(LambdaTask task, CancellationToken token)
        {
            await SendObjectAsync(new TaskUpdated(task), token);
        }

        public event Action OnClose;

        private async Task Handshake(CancellationToken cancellationToken)
        {
            var _ = await _socket.ReceiveObjectAsync<ClientHandshake>(cancellationToken);
            await _socket.SendObjectAsync(new ServerHandshake(new ServerHandshake.ServerHandshakePayload
            {
                ServerId = Guid.Empty,
                ServerName = Environment.MachineName,
                SessionId = SessionId
            }), cancellationToken);
            _isHandshakeAccepted = true;
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

        private async Task SendObjectAsync<T>(T obj, CancellationToken cancellationToken)
        {
            await _socket.SendObjectAsync(obj, cancellationToken);
        }
    }

    public class WebSocketSessionConfiguration
    {
        public int HeartbeatInterval { get; set; }
    }
}