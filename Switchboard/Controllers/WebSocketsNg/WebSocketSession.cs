using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Controllers.WebSocketized.Contracts;

namespace Switchboard.Controllers.WebSocketsNg
{
    internal class WebSocketSession : WebSocketSessionBase
    {
        public WebSocketSession(WebSocket socket, MemoryStreamPool memoryStreamPool) : base(socket, memoryStreamPool)
        {
        }

        protected override async Task<bool> TryHandleMessage(CancellationToken cancellationToken)
        {
            var ctx = await Socket.ReceiveObjectAsync(cancellationToken);
            var message = await ctx.DeserializeAsync<Message>(cancellationToken);
            switch (message.OperationCode)
            {
                case OperationCode.Close:
                    await Socket.EnsureClosedAsync(WebSocketCloseStatus.NormalClosure,
                        "Closure requested by the client", cancellationToken);
                    return false;
                case OperationCode.Handshake:
                    await Socket.EnsureClosedAsync(WebSocketCloseStatus.ProtocolError,
                        "Session has already completed handshake", cancellationToken);
                    return false;
                case OperationCode.Heartbeat:
                    // TODO: implements heartbeat
                    return true;
                case OperationCode.TaskInit:
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}