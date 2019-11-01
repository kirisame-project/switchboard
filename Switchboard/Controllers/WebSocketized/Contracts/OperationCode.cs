namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public enum OperationCode
    {
        Close = 1,
        Handshake = 2,
        Heartbeat = 3
    }
}
