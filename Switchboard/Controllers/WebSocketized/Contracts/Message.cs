using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts
{
    internal class Message
    {
        [Obsolete("For deserialization only")]
        public Message() : this(0)
        {
        }

        protected Message(int opCode)
        {
            OperationCode = opCode;
        }

        [JsonPropertyName("op")] public int OperationCode { get; set; }
    }
}