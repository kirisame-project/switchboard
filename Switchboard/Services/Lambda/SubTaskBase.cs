using System;
using System.Text.Json.Serialization;

namespace Switchboard.Services.Lambda
{
    public abstract class SubTaskBase
    {
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SubTaskState State { get; set; }

        [JsonPropertyName("id")] public Guid SubTaskId { get; set; }

        [JsonPropertyName("_time")] public int Time { get; set; }
    }
}