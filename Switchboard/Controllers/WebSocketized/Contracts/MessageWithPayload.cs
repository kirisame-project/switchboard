using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    public class MessageWithPayload<T> : Message
    {
        [Obsolete("For deserialization only")]
        public MessageWithPayload() : base(0)
        {
        }

        protected MessageWithPayload(int opCode, T payload) : base(opCode)
        {
            Payload = payload;
        }

        [JsonPropertyName("data")] public T Payload { get; set; }
    }
}