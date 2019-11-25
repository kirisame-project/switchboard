using System;
using System.Text.Json.Serialization;

namespace Switchboard.Services.FaceRecognition
{
    internal class RecognizedFace
    {
        [JsonPropertyName("_id")] public Guid Id { get; set; }

        [JsonPropertyName("position")] public FacePosition Position { get; set; }

        [JsonPropertyName("results")] public FaceSearchResult[] SearchResults { get; set; }

        [JsonPropertyName("vector")] public double[] FeatureVector { get; set; }
    }
}