using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts.Common
{
    internal class Heartbeat : MessageWithPayload<Heartbeat.Content>
    {
        [Obsolete("For deserialization only")]
        public Heartbeat() : base((int) OperationCodes.SessionHeartbeat, null)
        {
        }

        public Heartbeat(Guid sessionId) : base((int) OperationCodes.SessionHeartbeat, new Content(sessionId))
        {
        }

        internal class Content
        {
            [Obsolete("For deserialization only")]
            public Content()
            {
            }

            public Content(Guid sessionId)
            {
                SessionId = sessionId;
            }

            [JsonPropertyName("sessionId")] public Guid SessionId { get; set; }
        }
    }
}