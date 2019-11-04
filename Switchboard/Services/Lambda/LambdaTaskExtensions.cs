using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Switchboard.Services.Upstream;

namespace Switchboard.Services.Lambda
{
    public static class LambdaTaskExtensions
    {
        private static Stream CropFaceImage(this LambdaFace face, Image originalImage)
        {
            var height = face.Position.Y2 - face.Position.Y1;
            var width = face.Position.X2 - face.Position.X1;

            var stream = new MemoryStream();
            originalImage
                .Clone(ctx => ctx.Crop(new Rectangle(face.Position.X1, face.Position.Y1, width, height)))
                .SaveAsJpeg(stream);
            return stream;
        }

        private static async Task UpdateFaceImages(this LambdaTask task)
        {
            foreach (var face in task.Faces)
                task.VectorSubTask.FaceImages[face.FaceId] = await Task.Run(() =>
                    CropFaceImage(face, task.OriginalImageInstance)
                );
        }

        public static async Task RunDetection(this LambdaTask task, IUpstreamService upstream,
            CancellationToken cancellationToken)
        {
            var time = DateTime.Now;
            try
            {
                task.DetectionSubTask.State = SubTaskState.Running;
                var results = await upstream.FindFacesV2(task.OriginalImage, cancellationToken);
                var maxHeight = task.OriginalImageInstance.Height;
                var maxWidth = task.OriginalImageInstance.Width;
                task.Faces = results.Select(position =>
                {
                    var face = new LambdaFace(position);
                    face.Position.X1 = Math.Max(face.Position.X1, 0);
                    face.Position.X2 = Math.Min(face.Position.X2, maxWidth);
                    face.Position.Y1 = Math.Max(face.Position.Y1, 0);
                    face.Position.Y2 = Math.Min(face.Position.Y2, maxHeight);
                    return face;
                }).ToArray();
                task.DetectionSubTask.State = SubTaskState.Completed;
            }
            catch
            {
                task.DetectionSubTask.State = SubTaskState.Failed;
                throw;
            }
            finally
            {
                task.DetectionSubTask.Time = (int) (DateTime.Now - time).TotalMilliseconds;
            }
        }

        public static async Task RunVectoring(this LambdaTask task, IUpstreamService upstream,
            CancellationToken cancellationToken)
        {
            var time = DateTime.Now;
            try
            {
                task.VectorSubTask.State = SubTaskState.Running;

                // crop faces in sequence
                await UpdateFaceImages(task);

                // request vector in parallel
                await Task.WhenAll(task.Faces.Select(async face =>
                {
                    var faceImage = task.VectorSubTask.FaceImages[face.FaceId];
                    face.FeatureVector = await upstream.GetFaceFeatureVector(faceImage, cancellationToken);
                }));

                task.VectorSubTask.State = SubTaskState.Completed;
            }
            catch
            {
                task.VectorSubTask.State = SubTaskState.Failed;
                throw;
            }
            finally
            {
                task.VectorSubTask.Time = (int) (DateTime.Now - time).TotalMilliseconds;
            }
        }

        public static async Task RunSearch(this LambdaTask task, IUpstreamService upstream,
            CancellationToken cancellationToken)
        {
            var time = DateTime.Now;
            try
            {
                task.SearchSubTask.State = SubTaskState.Running;
                var request = task.Faces.ToDictionary(f => f.FaceId.ToString(), f => f.FeatureVector);
                var result = await upstream.SearchFacesByFeatureVectors(request, cancellationToken);
                foreach (var face in task.Faces) face.SearchResult = result[face.FaceId.ToString()];
                task.SearchSubTask.State = SubTaskState.Completed;
            }
            catch
            {
                task.SearchSubTask.State = SubTaskState.Failed;
                throw;
            }
            finally
            {
                task.SearchSubTask.Time = (int) (DateTime.Now - time).TotalMilliseconds;
            }
        }
    }
}