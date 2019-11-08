using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Controllers.WebSocketized;
using Switchboard.Controllers.WebSocketized.Contracts;
using Switchboard.Services.Lambda;

namespace Switchboard.Controllers.WebSocketsNg
{
    internal abstract class WebSocketSessionBase : IWebSocketSession
    {
        protected readonly WebSocketShim Socket;

        protected WebSocketSessionBase(WebSocket socket, MemoryStreamPool memoryStreamPool)
        {
            Socket = new WebSocketShim(socket, memoryStreamPool);
            SessionId = Guid.NewGuid();
        }

        public Guid SessionId { get; }

        public void Dispose()
        {
            Socket?.Dispose();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Handshake(cancellationToken);
                SessionActive = true;

                while (!cancellationToken.IsCancellationRequested)
                    if (!await TryHandleMessage(cancellationToken))
                        return;

                await Socket.EnsureClosedAsync(WebSocketCloseStatus.InternalServerError, "Session cancelled",
                    CancellationToken.None);
                throw new OperationCanceledException(cancellationToken);
            }
            catch (NotImplementedException)
            {
                await Socket.EnsureClosedAsync(WebSocketCloseStatus.ProtocolError,
                    "Requested operation not implemented", cancellationToken);
                if (Debugger.IsAttached) throw;
            }
            catch (WebSocketException)
            {
                await Socket.EnsureClosedAsync(WebSocketCloseStatus.ProtocolError, "WebSocket operation failed",
                    cancellationToken);
                if (Debugger.IsAttached) throw;
            }
            catch
            {
                await Socket.EnsureClosedAsync(WebSocketCloseStatus.InternalServerError, "Internal server error",
                    cancellationToken);
                throw;
            }
            finally
            {
                SessionActive = false;
            }
        }

        public async Task SendTaskUpdateAsync(LambdaTask task, CancellationToken cancellationToken)
        {
            await Socket.SendObjectAsync(new TaskUpdated(task), cancellationToken);
        }

        public bool SessionActive { get; private set; }

        protected abstract Task<bool> TryHandleMessage(CancellationToken cancellationToken);

        private async Task Handshake(CancellationToken cancellationToken)
        {
            var _ = await Socket.ReceiveObjectAsync<ClientHandshake>(cancellationToken);

            var serverHandshake = new ServerHandshake(new ServerHandshake.ServerHandshakePayload
            {
                ServerId = Guid.Empty,
                ServerName = Environment.MachineName,
                SessionId = SessionId
            });
            await Socket.SendObjectAsync(serverHandshake, cancellationToken);
        }
    }
}