using Switchboard.Controllers.WebSocketized.Contracts;

namespace Switchboard.Controllers.WebSocketsX.Facilities.Attributes
{
    internal class WebSocketOperationHandlerContext
    {
        public WebSocketOperationHandlerContext(Message message, WebSocketShim socket)
        {
            Message = message;
            Socket = socket;
        }

        public Message Message { get; }

        public WebSocketShim Socket { get; }
    }
}