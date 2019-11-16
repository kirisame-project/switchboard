namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class Heartbeat : Message
    {
        public Heartbeat() : base((int) OperationCodes.Handshake)
        {
        }
    }
}