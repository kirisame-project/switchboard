using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Controllers.WebSocketsX.Facilities;
using Switchboard.Controllers.WebSocketsX.Facilities.Buffers;

namespace Switchboard.Controllers.WebSocketsX
{
    internal class WebSocketController : WebSocketControllerBase
    {
        public WebSocketController(WebSocket socket, MemoryStreamPool msPool) : base(socket, msPool)
        {
        }

        protected override Task<bool> Handshake(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}