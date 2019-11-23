using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Common;
using Switchboard.Services.Lambda;
using Switchboard.Services.Upstream;

namespace Switchboard.Services.FaceRecognition
{
    [Component(ComponentLifestyle.Singleton, Implements = typeof(IFaceRecognitionService))]
    internal class HttpFaceRecognitionService : IFaceRecognitionService
    {
        private readonly IUpstreamService _upstreamService;

        public HttpFaceRecognitionService(IUpstreamService upstreamService)
        {
            _upstreamService = upstreamService;
        }

        public RecognitionTask RequestRecognition(Stream imageStream, Func<Task> onUpdate)
        {
            var task = new RecognitionTask(imageStream);

            Task.Run(async () =>
            {
                try
                {
                    await task.RunDetection(_upstreamService, CancellationToken.None);
                    await onUpdate.Invoke();

                    if (task.FaceCount > 0)
                    {
                        await task.RunVectoring(_upstreamService, CancellationToken.None);
                        await task.RunSearch(_upstreamService, CancellationToken.None);
                        await onUpdate.Invoke();
                    }
                }
                finally
                {
                    imageStream.Dispose();
                }
            });

            return task;
        }
    }
}