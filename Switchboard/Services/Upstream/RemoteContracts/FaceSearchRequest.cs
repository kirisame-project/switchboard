using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Switchboard.Services.Upstream.RemoteContracts
{
    internal class FaceSearchRequest
    {
        [JsonPropertyName("count")] public int Count { get; set; }

        [JsonPropertyName("topk")] public int CandidateCount { get; set; }

        [JsonPropertyName("vectors")] public IDictionary<string, double[]> Vectors { get; set; }
    }
}