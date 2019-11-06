using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Switchboard.Controllers.ResponseContracts;
using Switchboard.Controllers.WebSocketized;
using Switchboard.Metrics;
using Switchboard.Services.Lambda;
using Switchboard.Services.Upstream;

namespace Switchboard.Controllers
{
    [ApiController]
    [Route("api/v1/lambda")]
    public class HttpApiController : ControllerBase
    {
        private static readonly MeasurementOptions DetectionTime = new MeasurementOptions
        {
            Name = "sub_task_detection_time"
        };

        private static readonly MeasurementOptions VectorTime = new MeasurementOptions
        {
            Name = "sub_task_vector_time"
        };

        private static readonly MeasurementOptions SearchTime = new MeasurementOptions
        {
            Name = "sub_task_search_time"
        };

        private static readonly MeasurementOptions Stage1ResponseTime = new MeasurementOptions
        {
            Name = "stage1_response_time"
        };

        private static readonly MeasurementOptions Stage2ResponseTime = new MeasurementOptions
        {
            Name = "stage2_response_time"
        };

        private static readonly MeasurementOptions FaceCount = new MeasurementOptions
        {
            Name = "face_count"
        };

        private readonly MeasurementWriterFactory _metrics;
        private readonly IUpstreamService _upstreamService;
        private readonly WebSocketController _websockets;

        public HttpApiController(IUpstreamService upstreamService, WebSocketController websockets,
            MeasurementWriterFactory metrics)
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
            var startTime = DateTime.Now;

            // test if websocket session exists
            if (!_websockets.TryGetSession(sessionId, out var session) || !session.SessionActive)
                return new BadRequestObjectResult(new ErrorResponse(400, "WebSocket session not found"));

            var metrics = _metrics.GetInstance(new Dictionary<string, string>
            {
                {"sessionId", sessionId.ToString()}
            });

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
            metrics.Write(DetectionTime, task.DetectionSubTask.Time);
            metrics.Write(FaceCount, task.FaceCount);

            // defer vectoring and search
            if (task.FaceCount > 0)
            {
                var _ = Task.Run(async () =>
                {
                    await task.RunVectoring(_upstreamService, token);
                    await task.RunSearch(_upstreamService, token);
                    task.Time = (int) (DateTime.Now - task.CreationTime).TotalMilliseconds;
                    await session.SendTaskUpdateAsync(task, token);

                    metrics.Write(VectorTime, task.VectorSubTask.Time);
                    metrics.Write(SearchTime, task.SearchSubTask.Time);
                    metrics.Write(Stage2ResponseTime, (int) (DateTime.Now - startTime).TotalMilliseconds);
                }, token);
            }

            metrics.Write(Stage1ResponseTime, (int) (DateTime.Now - startTime).TotalMilliseconds);
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