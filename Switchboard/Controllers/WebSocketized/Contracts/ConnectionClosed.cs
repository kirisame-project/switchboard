using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class ConnectionClosed : Message
    {
        [JsonPropertyName("reason")] public string Reason { get; set; }

        public ConnectionClosed(int code, string reason) : base(code)
        {
            
        }
    }
}