using System;
using System.Net.WebSockets;

namespace Switchboard.Controllers.WebSocketized
{
    [Obsolete]
    public class WebSocketOperationException : Exception
    {
        public WebSocketOperationException(string message,
            WebSocketCloseStatus code = WebSocketCloseStatus.InternalServerError) : base(message)
        {
            Code = code;
        }

        public WebSocketCloseStatus Code { get; }
    }
}