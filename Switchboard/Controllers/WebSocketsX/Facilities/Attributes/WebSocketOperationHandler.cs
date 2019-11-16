using System.Threading.Tasks;

namespace Switchboard.Controllers.WebSocketsX.Facilities.Attributes
{
    internal delegate Task WebSocketOperationHandler(WebSocketOperationHandlerContext context);
}