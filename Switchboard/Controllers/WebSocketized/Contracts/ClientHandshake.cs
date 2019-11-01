using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class ClientHandshake : MessageWithPayload<ClientHandshake.ClientHandshakePayload>
    {
        public class ClientHandshakePayload
        {
            [JsonPropertyName("id")] public Guid ClientId { get; set; }

            [JsonPropertyName("name")] public string ClientName { get; set; }
        }
    }
}