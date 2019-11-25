using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Switchboard.Services.FaceRecognition.Abstractions
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BaseTaskState
    {
        [EnumMember(Value = "pending")] Pending,
        [EnumMember(Value = "running")] Running,
        [EnumMember(Value = "completed")] Succeeded,
        [EnumMember(Value = "failed")] Failed
    }
}