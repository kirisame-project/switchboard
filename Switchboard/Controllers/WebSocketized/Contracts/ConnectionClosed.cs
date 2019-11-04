using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class ConnectionClosed
    {
        [JsonPropertyName("error")] public string Error { get; set; }

        [JsonPropertyName("reason")] public string Reason { get; set; }
    }
}