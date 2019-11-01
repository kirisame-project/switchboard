using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class Message
    {
        [JsonPropertyName("op")] public OperationCode OperationCode { get; set; }
    }
}
