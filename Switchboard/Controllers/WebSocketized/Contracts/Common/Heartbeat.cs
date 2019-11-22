using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts.Common
{
    internal class Heartbeat : MessageWithPayload<Heartbeat.Content>
    {
        public Heartbeat(Guid sessionId) : base((int) OperationCodes.SessionHeartbeat, null)
        {
        }

        internal class Content
        {
            [JsonPropertyName("sessionId")] public Guid SessionId { get; }
        }
    }
}