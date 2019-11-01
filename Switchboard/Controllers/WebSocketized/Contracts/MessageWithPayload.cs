using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class MessageWithPayload<T> : Message
    {
        [JsonPropertyName("data")] public T Payload { get; set; }
    }
}