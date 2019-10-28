using System;
using System.Text.Json.Serialization;

namespace Switchboard.Services.Lambda
{
    public class LambdaFace
    {
        public LambdaFace(FacePosition position)
        {
            Id = Guid.NewGuid();
            Position = position;
        }

        [JsonPropertyName("id")] public Guid Id { get; }

        [JsonPropertyName("_vector_time")] public int RecognitionTime { get; set; }

        [JsonPropertyName("search_result")] public FaceSearchResult SearchResult { get; set; }

        [JsonPropertyName("position")] public FacePosition Position { get; }

        [JsonPropertyName("vector")] public double[] FeatureVector { get; set; }
    }
}