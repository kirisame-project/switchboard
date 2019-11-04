using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Switchboard.Controllers.ResponseContracts;
using Switchboard.Controllers.WebSocketized;
using Switchboard.Services.Lambda;
using Switchboard.Services.Upstream;

namespace Switchboard.Controllers
{
    [ApiController]
    [Route("api/v1/lambda")]
    public class FaceServiceController : ControllerBase
    {
        private readonly IUpstreamService _upstreamService;
        private readonly WebSocketController _websockets;
        private readonly IMetrics _metrics;

        public FaceServiceController(IUpstreamService upstreamService, WebSocketController websockets, IMetrics metrics)
        {
            _upstreamService = upstreamService;
            _websockets = websockets;
            _metrics = metrics;
        }

        [Consumes("image/jpeg")]
        [HttpPost]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesResponseType(typeof(LambdaTask), StatusCodes.Status202Accepted)]
        [Route("")]
        public async Task<IActionResult> DoStandardRequest([FromHeader(Name = "X-WebSocket-Session-Id")]
            Guid sessionId)
        {
            var time = DateTime.Now;

            // test if websocket session exists
            if (!_websockets.TryGetSession(sessionId, out var session) || !session.IsSessionActive())
                return new BadRequestObjectResult(new ErrorResponse(400, "WebSocket session not found"));

            // retrieve uploaded image
            if (Request.Body == null)
                return new BadRequestObjectResult(new ErrorResponse(400, "Image payload cannot be empty"));
            var image = new MemoryStream();
            await Request.Body.CopyToAsync(image);

            // create task
            var task = new LambdaTask(image);

            // start detection task
            var token = CancellationToken.None;
            await task.RunDetection(_upstreamService, token);

            _metrics.Measure.Histogram.Update(MetricsRegistry.StandardRequestFaceCount, task.FaceCount);
            _metrics.Measure.Histogram.Update(MetricsRegistry.StandardRequestDetectionTime, task.DetectionSubTask.Time);

            // defer vectoring and search
            if (task.FaceCount > 0)
            {
                var _ = Task.Run(async () =>
                {
                    await task.RunVectoring(_upstreamService, token);
                    await task.RunSearch(_upstreamService, token);
                    task.Time = (int) (DateTime.Now - task.CreationTime).TotalMilliseconds;
                    await session.SendTaskUpdateAsync(task, token);

                    _metrics.Measure.Histogram.Update(MetricsRegistry.StandardRequestVectorTime, task.VectorSubTask.Time);
                    _metrics.Measure.Histogram.Update(MetricsRegistry.StandardRequestSearchTime, task.SearchSubTask.Time);

                    var stage2Time = (int) (DateTime.Now - time).TotalMilliseconds;
                    _metrics.Measure.Histogram.Update(MetricsRegistry.StandardRequestStage2Time, stage2Time);
                }, token);
            }

            var stage1Time = (int) (DateTime.Now - time).TotalMilliseconds;
            _metrics.Measure.Histogram.Update(MetricsRegistry.StandardRequestStage1Time, stage1Time);

            return new OkObjectResult(task);
        }

        [Consumes("image/jpeg")]
        [HttpPost]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("full")]
        public async Task<IActionResult> DoFullRequest()
        {
            if (Request.Body == null)
                return new BadRequestObjectResult(new ErrorResponse(400, "Image payload cannot be empty"));

            var image = new MemoryStream();
            await Request.Body.CopyToAsync(image);

            var task = new LambdaTask(image);

            var token = CancellationToken.None; // TODO: use upstream timeout
            await task.RunDetection(_upstreamService, token);
            await task.RunVectoring(_upstreamService, token);
            await task.RunSearch(_upstreamService, token);

            return new OkObjectResult(task);
        }
    }
}