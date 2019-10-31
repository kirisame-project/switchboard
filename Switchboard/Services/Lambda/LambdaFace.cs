using System;
using System.Text.Json.Serialization;

namespace Switchboard.Services.Lambda
{
    public class LambdaFace
    {
        public LambdaFace(FacePosition position)
        {
            FaceId = Guid.NewGuid();
            Position = position;
        }

        [JsonPropertyName("id")] public Guid FaceId { get; set; }

        [JsonPropertyName("position")] public FacePosition Position { get; set; }

        [JsonPropertyName("vector")] public double[] FeatureVector { get; set; }

        [JsonPropertyName("searchResults")] public FaceSearchResult[] SearchResult { get; set; }
    }
}