using System.Text.Json.Serialization;

namespace Switchboard.Controllers.ResponseContracts
{
    internal class ErrorResponse
    {
        public ErrorResponse(int code, string reason)
        {
            Code = code;
            Reason = reason;
        }

        [JsonPropertyName("code")] public int Code { get; set; }

        [JsonPropertyName("reason")] public string Reason { get; set; }
    }
}