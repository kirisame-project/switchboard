using System;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Controllers.WebSocketized.Abstractions;
using Switchboard.Controllers.WebSocketized.Contracts;
using Switchboard.Controllers.WebSocketized.Contracts.Common;
using Switchboard.Controllers.WebSocketized.Contracts.Local;
using Switchboard.Controllers.WebSocketized.Contracts.Remote;
using Switchboard.Controllers.WebSocketsX.Facilities;
using Switchboard.Controllers.WebSocketsX.Facilities.Attributes;
using Switchboard.Controllers.WebSocketsX.Facilities.Buffers;
using Switchboard.Services.Lambda;

namespace Switchboard.Controllers.WebSocketsX
{
    internal class WebSocketSession : WebSocketController, IWebSocketSession
    {
        public WebSocketSession(MemoryStreamPool memoryStreamPool) : base(memoryStreamPool)
        {
            SessionId = Guid.NewGuid();
            SessionState = WebSocketSessionState.New;
        }

        public Guid SessionId { get; }

        public WebSocketSessionState SessionState { get; }

        public async Task SendTaskUpdateAsync(LambdaTask task, CancellationToken cancellationToken)
        {
            await Socket.SendObjectAsync(new ImageTaskUpdate(task), cancellationToken);
        }

        [OperationHandler((int) OperationCodes.Close, typeof(CloseSession))]
        private async Task HandleCloseSession(OperationHandlerContext ctx, CancellationToken cancellationToken)
        {
            await Task.FromException(new NotImplementedException());
        }

        [OperationHandler((int) OperationCodes.SessionHandshake, typeof(ClientHandshake))]
        private async Task HandleClientHandshake(OperationHandlerContext ctx, CancellationToken cancellationToken)
        {
            await Task.FromException(new NotImplementedException());
        }

        [OperationHandler((int) OperationCodes.SessionHeartbeat, typeof(Heartbeat))]
        private async Task HandleClientHeartbeat(OperationHandlerContext ctx, CancellationToken cancellationToken)
        {
            await Socket.SendObjectAsync(new Heartbeat(SessionId), cancellationToken);
        }

        [OperationHandler((int) OperationCodes.ImageTaskRequest, typeof(ImageTaskRequest))]
        private async Task HandleClientRequest(OperationHandlerContext ctx, CancellationToken cancellationToken)
        {
            await Task.FromException(new NotImplementedException());
        }

        [OperationHandler((int) OperationCodes.ImageTaskUpdated, typeof(ImageTaskUpdate))]
        private async Task HandleServerOnlyOperation(OperationHandlerContext ctx, CancellationToken cancellationToken)
        {
            await Task.FromException(new NotImplementedException());
        }
    }
}