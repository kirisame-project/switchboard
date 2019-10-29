using System;
using System.IO;
using System.Text.Json.Serialization;

namespace Switchboard.Services.Lambda
{
    public class LambdaTask : IDisposable
    {
        public LambdaTask(MemoryStream image)
        {
            OriginalImage = image;
        }

        [JsonPropertyName("_detection_time")] public int DetectionTime { get; set; }
        
        [JsonPropertyName("_vector_time_total")] public int TotalVectorTime { get; set; }

        [JsonPropertyName("_search_time")] public int SearchTime { get; set; }
        
        [JsonPropertyName("count")] public int FaceCount => Faces.Length;

        [JsonPropertyName("faces")] public LambdaFace[] Faces { get; set; } = Array.Empty<LambdaFace>();

        [JsonIgnore] public MemoryStream OriginalImage { get; }

        public void Dispose()
        {
            OriginalImage.Dispose();
            foreach (var face in Faces) face.Dispose();
        }
    }
}