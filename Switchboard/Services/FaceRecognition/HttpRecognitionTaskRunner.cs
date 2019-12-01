using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AtomicAkarin.Shirakami.Reflections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Switchboard.Services.Common.Contracts;
using Switchboard.Services.FaceRecognition.Abstractions;
using Switchboard.Services.Upstream;

namespace Switchboard.Services.FaceRecognition
{
    [Component(ServiceLifetime.Singleton, ServiceType = typeof(IRecognitionTaskRunner))]
    [ExternalComponent(typeof(RecyclableMemoryStreamManager), ServiceLifetime.Singleton)]
    internal class HttpRecognitionTaskRunner : IRecognitionTaskRunner
    {
        private readonly ILogger _logger;
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;

        private readonly IUpstreamService _upstream;

        public HttpRecognitionTaskRunner(IUpstreamService upstream, ILoggerFactory loggerFactory,
            RecyclableMemoryStreamManager memoryStreamManager)
        {
            _upstream = upstream;
            _memoryStreamManager = memoryStreamManager;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task RunTaskAsync(RecognitionTask task, CancellationToken cancellationToken)
        {
            task.State = BaseTaskState.Running;
            try
            {
                await RunDetection(task, cancellationToken);
                if (task.FaceCount > 0)
                {
                    await RunVectorization(task, cancellationToken);
                    await RunSearch(task, cancellationToken);
                }

                task.State = BaseTaskState.Succeeded;
            }
            catch (Exception e)
            {
                if (e is OperationCanceledException && !cancellationToken.IsCancellationRequested)
                    _logger.LogWarning("Upstream service timed out, or too slow to respond\n" +
                                       $"Detection.State={task.DetectionTask.State}, Time={task.DetectionTask.Time}\n" +
                                       $"Vectorization.State={task.VectorizationTask.State}, Time={task.VectorizationTask.Time}\n" +
                                       $"Search.State={task.SearchTask.State}, Time={task.SearchTask.Time}");

                task.State = BaseTaskState.Failed;
                throw;
            }
        }

        private Stream CropFaceImage(RecognizedFace face, Image originalImage)
        {
            var height = face.Position.Y2 - face.Position.Y1;
            var width = face.Position.X2 - face.Position.X1;

            var stream = _memoryStreamManager.GetStream("face_crop_stream");
            originalImage
                .Clone(ctx => ctx.Crop(new Rectangle(face.Position.X1, face.Position.Y1, width, height)))
                .SaveAsJpeg(stream);
            return stream;
        }

        private async Task RunDetection(RecognitionTask task, CancellationToken cancellationToken)
        {
            task.DetectionTask.State = BaseTaskState.Running;
            try
            {
                var results = await _upstream.FindFacesV2(task.ImageStream, cancellationToken);
                var maxHeight = task.ImageInstance.Height;
                var maxWidth = task.ImageInstance.Width;
                task.Faces = results.Select(position => new RecognizedFace
                {
                    Position = new FacePosition
                    {
                        X1 = Math.Max(position.X1, 0),
                        X2 = Math.Min(position.X2, maxWidth),
                        Y1 = Math.Max(position.Y1, 0),
                        Y2 = Math.Min(position.Y2, maxHeight)
                    }
                }).ToArray();

                task.DetectionTask.State = BaseTaskState.Succeeded;
            }
            catch
            {
                task.DetectionTask.State = BaseTaskState.Failed;
                throw;
            }
        }

        private async Task RunVectorization(RecognitionTask task, CancellationToken cancellationToken)
        {
            task.VectorizationTask.State = BaseTaskState.Running;
            try
            {
                await Task.WhenAll(task.Faces.Select(async face =>
                {
                    // crop image
                    var faceStream = await Task.Run(() => CropFaceImage(face, task.ImageInstance), cancellationToken);

                    // vectorization
                    face.FeatureVector = await _upstream.GetFaceFeatureVector(faceStream, cancellationToken);
                }));

                task.VectorizationTask.State = BaseTaskState.Succeeded;
            }
            catch
            {
                task.VectorizationTask.State = BaseTaskState.Failed;
                throw;
            }
        }

        private async Task RunSearch(RecognitionTask task, CancellationToken cancellationToken)
        {
            task.SearchTask.State = BaseTaskState.Running;
            try
            {
                var request = task.Faces.ToDictionary(f => f.Id.ToString(), f => f.FeatureVector);
                var result = await _upstream.SearchFacesByFeatureVectors(request, cancellationToken);
                foreach (var face in task.Faces) face.SearchResults = result[face.Id.ToString()];

                task.SearchTask.State = BaseTaskState.Succeeded;
            }
            catch
            {
                task.SearchTask.State = BaseTaskState.Failed;
                throw;
            }
        }
    }
}