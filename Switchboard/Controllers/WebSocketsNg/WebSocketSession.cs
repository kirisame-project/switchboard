using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Controllers.WebSocketized;
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
                        return;
                throw new OperationCanceledException(cancellationToken);
            }
            catch (WebSocketException)
            {
                throw new NotImplementedException("Unhandled WebSocket exceptions");
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
            throw new NotImplementedException();
        }

        private Task Handshake(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}