using System;

namespace Switchboard.Controllers.WebSocketsX.Facilities.Exceptions
{
    internal class WebSocketControllerException : Exception
    {
        public WebSocketControllerException(int code, string reason)
        {
            Code = code;
            Reason = reason;
        }

        public int Code { get; }

        public string Reason { get; }
    }
}