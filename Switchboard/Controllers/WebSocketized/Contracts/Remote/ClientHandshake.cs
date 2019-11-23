using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts.Remote
{
    internal class ClientHandshake : MessageWithPayload<ClientHandshake.Content>
    {
        [Obsolete("For deserialization only")]
        public ClientHandshake() : base(0, null)
        {
        }

        internal class Content
        {
            [JsonPropertyName("agent")] public string AgentString { get; set; }

            [JsonPropertyName("clientId")] public Guid ClientInstanceId { get; set; }
        }
    }
}