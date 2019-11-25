using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Switchboard.Controllers.ResponseContracts;
using Switchboard.Controllers.WebSocketized.Abstractions;
using Switchboard.Metrics;
using Switchboard.Services.FaceRecognition;
using Switchboard.Services.FaceRecognition.Abstractions;

namespace Switchboard.Controllers
{
    [ApiController]
    [Route("api/v1/lambda")]
    internal class HttpApiController : ControllerBase
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


        private readonly IRecognitionTaskRunner _taskRunner;

        private readonly IWebSocketSessionHub _websockets;

        public HttpApiController(IRecognitionTaskRunner taskRunner, IWebSocketSessionHub websockets,
            MeasurementWriterFactory metrics)
        {
            _taskRunner = taskRunner;
            _websockets = websockets;
            _metrics = metrics;
        }

        [Consumes("image/jpeg")]
        [HttpPost]
        [ProducesErrorResponseType(typeof(ErrorResponse))]
        [ProducesResponseType(typeof(RecognitionTask), StatusCodes.Status202Accepted)]
        [Route("")]
        public async Task<IActionResult> DoStandardRequest([FromHeader(Name = "X-WebSocket-Session-Id")]
            Guid sessionId)
        {
            var startTime = DateTime.Now;

            if (!_websockets.TryGetSession(sessionId, out var session))
                return new BadRequestObjectResult(new ErrorResponse(400, "WebSocket session not found"));

            if (session.SessionState != WebSocketSessionState.SessionEstablished)
                return new BadRequestObjectResult(new ErrorResponse(400, "Invalid WebSocket session state"));

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
            var task = new RecognitionTask(image);
            var cancellationToken = CancellationToken.None;

            // subscribe detection task
            var detectionTask = new TaskCompletionSource<bool>();
            task.DetectionTask.OnStateChanged += (sender, arg) => detectionTask.SetResult(true);

            // subscribe entire task
            task.OnStateChanged += (sender, arg) =>
            {
                if (task.State != BaseTaskState.Succeeded && task.State != BaseTaskState.Failed) return;

                session.SendTaskUpdateAsync(task, cancellationToken);

                metrics.Write(VectorTime, task.VectorizationTask.Time);
                metrics.Write(SearchTime, task.SearchTask.Time);

                metrics.Write(Stage2ResponseTime, (int) (DateTime.Now - startTime).TotalMilliseconds);
            };

            // start entire task
            var _ = _taskRunner.RunTaskAsync(task, cancellationToken);

            // await detection task
            await detectionTask.Task;
            metrics.Write(FaceCount, task.FaceCount);
            metrics.Write(DetectionTime, task.DetectionTask.Time);

            // return
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

            var task = new RecognitionTask(image);
            await _taskRunner.RunTaskAsync(task, CancellationToken.None);

            return new OkObjectResult(task);
        }
    }
}