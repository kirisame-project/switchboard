using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class MessageWithPayload<T> : Message
    {
        public MessageWithPayload(int opCode) : base(opCode)
        {
        }

        [JsonPropertyName("data")] public T Payload { get; set; }
    }
}