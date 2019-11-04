namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class Heartbeat : Message
    {
        public Heartbeat()
        {
            OperationCode = OperationCode.Heartbeat;
        }
    }
}