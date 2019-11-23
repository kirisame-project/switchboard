using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Controllers.WebSocketized.Abstractions
{
    internal interface IWebSocketSessionHub
    {
        Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken);

        bool TryGetSession(Guid sessionId, out IWebSocketSession session);
    }
}