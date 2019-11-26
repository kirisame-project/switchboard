using System;
using System.Text.Json.Serialization;

namespace Switchboard.Services.Common.Contracts
{
    internal class RecognizedFace
    {
        [JsonPropertyName("_id")] public Guid Id { get; set; } = Guid.NewGuid();

        [JsonPropertyName("position")] public FacePosition Position { get; set; } = new FacePosition();

        [JsonPropertyName("vector")] public double[] FeatureVector { get; set; } = Array.Empty<double>();

        [JsonPropertyName("results")]
        public FaceSearchResult[] SearchResults { get; set; } = Array.Empty<FaceSearchResult>();
    }
}