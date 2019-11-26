using System;
using System.IO;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using Switchboard.Services.Common.Contracts;
using Switchboard.Services.FaceRecognition.Abstractions;

namespace Switchboard.Services.FaceRecognition
{
    internal class RecognitionTask : BaseTask, IDisposable
    {
        private Image _image;

        public RecognitionTask(Stream image)
        {
            ImageStream = image;

            DetectionTask = new BaseTask();
            VectorizationTask = new BaseTask();
            SearchTask = new BaseTask();
        }

        [JsonPropertyName("faces")] public RecognizedFace[] Faces { get; set; }

        [JsonPropertyName("faceCount")] public int FaceCount => Faces.Length;

        [JsonPropertyName("detection")] public BaseTask DetectionTask { get; }

        [JsonPropertyName("vector")] public BaseTask VectorizationTask { get; }

        [JsonPropertyName("search")] public BaseTask SearchTask { get; }

        [JsonIgnore] public Image ImageInstance => OpenImage();

        [JsonIgnore] public Stream ImageStream { get; }

        public void Dispose()
        {
            ImageInstance?.Dispose();
            ImageStream?.Dispose();
        }

        private Image OpenImage()
        {
            if (_image != null)
                return _image;

            ImageStream.Seek(0, SeekOrigin.Begin);
            _image = Image.Load(ImageStream);
            return _image;
        }
    }
}