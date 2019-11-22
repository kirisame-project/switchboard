using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts.Local
{
    internal class ServerHandshake : MessageWithPayload<ServerHandshake.Content>
    {
        public ServerHandshake(Content content) : base((int) OperationCodes.SessionHandshake, content)
        {
        }

        internal class Content
        {
            [JsonPropertyName("serverId")] public Guid ServerInstanceId { get; set; }

            [JsonPropertyName("serverName")] public string ServerInstanceName { get; set; }

            [JsonPropertyName("sessionId")] public Guid SessionId { get; set; }
        }
    }
}