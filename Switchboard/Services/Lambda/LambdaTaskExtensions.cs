using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Services.Upstream;

namespace Switchboard.Services.Lambda
{
    public static class LambdaTaskExtensions
    {
        private static MemoryStream GetFaceImage(this LambdaFace face, Image originalImage)
        {
            var height = face.Position.Y2 - face.Position.Y1;
            var width = face.Position.X2 - face.Position.X1;

            using var output = new Bitmap(width, height);
            output.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

            using var canvas = Graphics.FromImage(output);
            canvas.CompositingQuality = CompositingQuality.HighQuality;
            canvas.InterpolationMode = InterpolationMode.HighQualityBilinear;
            canvas.SmoothingMode = SmoothingMode.HighQuality;
            canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var destRect = new Rectangle(0, 0, width, height);
            canvas.DrawImage(originalImage, destRect, face.Position.X1, face.Position.Y1, width, height,
                GraphicsUnit.Pixel);

            var outputStream = new MemoryStream();
            output.Save(outputStream, ImageFormat.Jpeg);

            return outputStream;
        }

        private static async Task UpdateFaceImage(this LambdaFace face, Image originalImage)
        {
            await Task.Run(() => face.FaceImage = face.GetFaceImage(originalImage));
        }

        public static async Task UpdateFacePositions(this LambdaTask task, IUpstreamService upstream,
            CancellationToken cancellationToken)
        {
            var time = DateTime.Now;
            var results = await upstream.FindFacesV2(task.OriginalImage, cancellationToken);
            task.Faces = results.Select(position => new LambdaFace(position)).ToArray();
            task.DetectionTime = (int) (DateTime.Now - time).TotalMilliseconds;
        }

        private static async Task UpdateFaceFeatureVector(this LambdaFace face,
            IUpstreamService upstream, CancellationToken cancellationToken)
        {
            var time = DateTime.Now;
            Debug.Assert(face.FaceImage != null);
            face.FeatureVector = await upstream.GetFaceFeatureVector(face.FaceImage, cancellationToken);
            face.RecognitionTime = (int) (DateTime.Now - time).TotalMilliseconds;
        }

        public static async Task UpdateFaceFeatureVectors(this LambdaTask task, IUpstreamService upstream,
            CancellationToken cancellationToken)
        {
            var time = DateTime.Now;

            // crop faces in sequence
            task.OriginalImage.Seek(0, SeekOrigin.Begin);
            var image = Image.FromStream(task.OriginalImage);
            foreach (var face in task.Faces) await face.UpdateFaceImage(image);

            // request vector in parallel
            await Task.WhenAll(task.Faces.Select(face => face.UpdateFaceFeatureVector(upstream, cancellationToken)));

            task.TotalVectorTime = (int) (DateTime.Now - time).TotalMilliseconds;
        }

        public static async Task UpdateFaceSearchResults(this LambdaTask task, IUpstreamService upstream,
            CancellationToken cancellationToken)
        {
            var time = DateTime.Now;
            var request = task.Faces.ToDictionary(f => f.Id.ToString(), f => f.FeatureVector);
            var result = await upstream.SearchFacesByFeatureVectors(request, cancellationToken);
            foreach (var face in task.Faces) face.SearchResult = result[face.Id.ToString()];
            task.SearchTime = (int) (DateTime.Now - time).TotalMilliseconds;
        }
    }
}