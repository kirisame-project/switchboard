using System;
using System.IO;
using System.Text.Json.Serialization;

namespace Switchboard.Services.Lambda
{
    public class LambdaTask : IDisposable
    {
        public LambdaTask(Stream image)
        {
            OriginalImage = image;

            CreationTime = DateTime.Now;
            DetectionSubTask = new DetectionSubTask {State = SubTaskState.Pending};
            VectorSubTask = new VectorSubTask {State = SubTaskState.Pending};
            SearchSubTask = new SearchSubTask {State = SubTaskState.Pending};
        }

        [JsonPropertyName("count")] public int FaceCount => Faces.Length;

        [JsonPropertyName("faces")] public LambdaFace[] Faces { get; set; } = Array.Empty<LambdaFace>();

        [JsonPropertyName("timestamp")] public DateTime CreationTime { get; set; }

        [JsonPropertyName("taskDetection")] public DetectionSubTask DetectionSubTask { get; set; }

        [JsonPropertyName("taskVector")] public VectorSubTask VectorSubTask { get; set; }

        [JsonPropertyName("taskSearch")] public SearchSubTask SearchSubTask { get; set; }

        [JsonIgnore] public Stream OriginalImage { get; }

        public void Dispose()
        {
            OriginalImage.Dispose();
        }
    }
}
