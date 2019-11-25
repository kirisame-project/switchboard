using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
using Switchboard.Controllers.WebSocketized.Abstractions;
using Switchboard.Controllers.WebSocketized.Contracts;
using Switchboard.Controllers.WebSocketized.Contracts.Common;
using Switchboard.Controllers.WebSocketized.Contracts.Local;
using Switchboard.Controllers.WebSocketized.Contracts.Remote;
using Switchboard.Controllers.WebSocketsX.Facilities;
using Switchboard.Controllers.WebSocketsX.Facilities.Attributes;
using Switchboard.Controllers.WebSocketsX.Facilities.Exceptions;
using Switchboard.Services.FaceRecognition;
using Switchboard.Services.FaceRecognition.Abstractions;

namespace Switchboard.Controllers.WebSocketsX
{
    internal class WebSocketSession : WebSocketController, IWebSocketSession
    {
        private readonly IRecognitionTaskRunner _taskRunner;

        public WebSocketSession(IRecognitionTaskRunner taskRunner, RecyclableMemoryStreamManager memoryStreamManager) :
            base(memoryStreamManager)
        {
            _taskRunner = taskRunner;
            SessionId = Guid.NewGuid();
            SessionState = WebSocketSessionState.New;
        }

        public Guid SessionId { get; }

        public WebSocketSessionState SessionState { get; private set; }

        public async Task SendTaskUpdateAsync(RecognitionTask task, CancellationToken cancellationToken)
        {
            await Socket.SendObjectAsync(new ImageTaskUpdate(task), cancellationToken);
        }

        public override Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            SessionState = WebSocketSessionState.ConnectionOpen;
            return base.AcceptAsync(socket, cancellationToken);
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            if (!await Handshake(cancellationToken))
                return;

            await base.RunAsync(cancellationToken);
        }

        private async Task<bool> Handshake(CancellationToken cancellationToken)
        {
            var handshake = await Socket.ReceiveObjectAsync<ClientHandshake>(cancellationToken);
            if (handshake.OperationCode != (int) OperationCodes.SessionHandshake)
            {
                await Socket.SendObjectAsync(new CloseSession(4402, "Invalid or Missing Handshake"), cancellationToken);
                await Socket.EnsureClosedAsync(WebSocketCloseStatus.ProtocolError, "Invalid or Missing Handshake",
                    cancellationToken);
                return false;
            }

            await Socket.SendObjectAsync(new ServerHandshake(new ServerHandshake.Content
            {
                SessionId = SessionId,
                ServerInstanceId = Guid.Empty,
                ServerInstanceName = Environment.MachineName
            }), cancellationToken);
            SessionState = WebSocketSessionState.SessionEstablished;
            return true;
        }

        [OperationHandler((int) OperationCodes.Close, typeof(CloseSession))]
        private Task HandleCloseSession(OperationHandlerContext ctx, CancellationToken cancellationToken)
        {
            return Task.FromException(new WebSocketControllerException(4200, "Closure Requested by Client"));
        }

        [OperationHandler((int) OperationCodes.SessionHandshake, typeof(ClientHandshake))]
        private Task HandleClientHandshake(OperationHandlerContext ctx, CancellationToken cancellationToken)
        {
            return Task.FromException(new WebSocketControllerException(4405, "Server-only Operations"));
        }

        [OperationHandler((int) OperationCodes.SessionHeartbeat, typeof(Heartbeat))]
        private async Task HandleClientHeartbeat(OperationHandlerContext ctx, CancellationToken cancellationToken)
        {
            await Socket.SendObjectAsync(new Heartbeat(SessionId), cancellationToken);
        }

        [OperationHandler((int) OperationCodes.ImageTaskRequest, typeof(ImageTaskRequest))]
        private async Task HandleClientImageRequest(OperationHandlerContext ctx, CancellationToken cancellationToken)
        {
            var request = ((ImageTaskRequest) ctx.Message).Payload;
            var image = await Socket.ReceiveStreamAsync(cancellationToken);

            var task = new RecognitionTask(image);

            void Callback(object sender, BaseTaskState arg)
            {
                if (!((BaseTask) sender).IsCompleted)
                    return;

                var _ = SendTaskUpdateAsync(task, cancellationToken);
            }

            task.DetectionTask.OnStateChanged += Callback;
            task.OnStateChanged += Callback;

            var _ = _taskRunner.RunTaskAsync(task, cancellationToken);
        }

        [OperationHandler((int) OperationCodes.ImageTaskUpdated, typeof(ImageTaskUpdate))]
        private Task HandleImageUpdate(OperationHandlerContext ctx, CancellationToken cancellationToken)
        {
            return Task.FromException(new WebSocketControllerException(4405, "Server-only Operations"));
        }
    }
}