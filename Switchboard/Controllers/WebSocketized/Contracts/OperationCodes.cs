namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public enum OperationCodes
    {
        Close = 1,
        Handshake = 2,
        Heartbeat = 3,
        TaskUpdated = 4,
        TaskInit = 5
    }
}