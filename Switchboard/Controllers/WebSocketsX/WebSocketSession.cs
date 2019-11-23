﻿using System;
using System.Net.WebSockets;
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
using Switchboard.Controllers.WebSocketsX.Facilities.Exceptions;
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

        public WebSocketSessionState SessionState { get; private set; }

        public async Task SendTaskUpdateAsync(LambdaTask task, CancellationToken cancellationToken)
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