using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Controllers.WebSocketized
{
    public interface IWebSocketRootController
    {
        Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken);

        bool TryGetSession(Guid sessionId, out IWebSocketController controller);
    }
}