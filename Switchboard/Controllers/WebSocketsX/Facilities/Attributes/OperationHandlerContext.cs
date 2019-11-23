using Switchboard.Controllers.WebSocketized.Contracts;

namespace Switchboard.Controllers.WebSocketsX.Facilities.Attributes
{
    internal class OperationHandlerContext
    {
        public OperationHandlerContext(Message message)
        {
            Message = message;
        }

        public Message Message { get; }
    }
}