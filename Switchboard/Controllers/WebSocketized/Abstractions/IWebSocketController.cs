using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Controllers.WebSocketized.Abstractions
{
    internal interface IWebSocketController
    {
        Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken);
    }
}