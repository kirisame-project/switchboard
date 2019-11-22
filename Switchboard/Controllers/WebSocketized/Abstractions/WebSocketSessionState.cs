namespace Switchboard.Controllers.WebSocketized.Abstractions
{
    internal enum WebSocketSessionState
    {
        New,
        ConnectionOpen,
        SessionEstablished,
        SessionClosed
    }
}