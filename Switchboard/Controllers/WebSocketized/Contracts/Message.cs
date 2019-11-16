using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class Message
    {
        public Message(int opCode)
        {
            OperationCode = opCode;
        }

        [JsonPropertyName("op")] public int OperationCode { get; }
    }
}