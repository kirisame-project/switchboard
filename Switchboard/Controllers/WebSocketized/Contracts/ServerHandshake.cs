using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class ServerHandshake : MessageWithPayload<ServerHandshake.ServerHandshakePayload>
    {
        public ServerHandshake(ServerHandshakePayload payload)
        {
            OperationCode = OperationCode.Handshake;
            Payload = payload;
        }

        public class ServerHandshakePayload
        {
            [JsonPropertyName("serverId")] public Guid ServerId { get; set; }

            [JsonPropertyName("serverName")] public string ServerName { get; set; }

            [JsonPropertyName("sessionId")] public Guid SessionId { get; set; }
        }
    }
}