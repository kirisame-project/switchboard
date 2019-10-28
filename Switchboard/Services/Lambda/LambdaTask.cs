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

        [JsonPropertyName("_search_time")] public int SearchTime { get; set; }

        [JsonPropertyName("faces")] public LambdaFace[] Faces { get; set; }

        [JsonIgnore] public MemoryStream OriginalImage { get; }

        public void Dispose()
        {
            OriginalImage.Dispose();
        }
    }
}