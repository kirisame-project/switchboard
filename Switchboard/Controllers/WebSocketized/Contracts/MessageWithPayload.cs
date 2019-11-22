using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class MessageWithPayload<T> : Message
    {
        public MessageWithPayload(int opCode, T payload) : base(opCode)
        {
            Payload = payload;
        }

        [JsonPropertyName("data")] public T Payload { get; }
    }
}