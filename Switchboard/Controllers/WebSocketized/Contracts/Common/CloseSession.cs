using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts.Common
{
    internal class CloseSession : Message
    {
        public CloseSession(int code, string reason) : base((int) OperationCodes.Close)
        {
            ClosureCode = code;
            ClosureReason = reason;
        }

        [Obsolete("For deserialization only")]
        public CloseSession() : base((int) OperationCodes.Close)
        {
        }

        [JsonPropertyName("code")] public int ClosureCode { get; set; }

        [JsonPropertyName("reason")] public string ClosureReason { get; set; }
    }
}