namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public enum OperationCodes
    {
        Close = 1,
        SessionHandshake = 2,
        SessionHeartbeat = 3,
        ImageTaskUpdated = 4,
        ImageTaskRequest = 5
    }
}