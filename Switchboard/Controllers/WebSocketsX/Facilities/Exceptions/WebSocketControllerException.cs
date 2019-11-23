using System;
using System.Net.WebSockets;

namespace Switchboard.Controllers.WebSocketsX.Facilities.Exceptions
{
    internal class WebSocketControllerException : Exception
    {
        public WebSocketControllerException(int code, string reason,
            WebSocketCloseStatus socketCode = WebSocketCloseStatus.Empty)
        {
            ClosureCode = code;
            ClosureReason = reason;
            WebSocketCloseStatus = socketCode;
        }

        public int ClosureCode { get; }

        public string ClosureReason { get; }

        public WebSocketCloseStatus WebSocketCloseStatus { get; }
    }
}