using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Controllers.WebSocketsX.Facilities.Attributes
{
    internal delegate Task OperationHandler(OperationHandlerContext context, CancellationToken cancellationToken);
}