using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts.Remote
{
    internal class ClientHandshake : MessageWithPayload<ClientHandshake.Content>
    {
        public ClientHandshake() : base((int) OperationCodes.SessionHandshake, null)
        {
        }

        internal class Content
        {
            [JsonPropertyName("agent")] public string AgentString { get; set; }

            [JsonPropertyName("clientId")] public Guid ClientInstanceId { get; set; }
        }
    }
}