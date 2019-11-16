using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Common;
using Switchboard.Controllers.WebSocketized.Contracts;
using Switchboard.Controllers.WebSocketsX.Facilities.Attributes;
using Switchboard.Controllers.WebSocketsX.Facilities.Buffers;
using Switchboard.Controllers.WebSocketsX.Facilities.Exceptions;

namespace Switchboard.Controllers.WebSocketsX.Facilities
{
    [Component(ComponentLifestyle.Singleton)]
    internal abstract class WebSocketControllerBase : IDisposable
    {
        private readonly IDictionary<int, (WebSocketOperationHandler, WebSocketOperationHandlerAttribute)> _handlers;

        protected readonly WebSocketShim Socket;

        protected WebSocketControllerBase(WebSocket socket, MemoryStreamPool msPool)
        {
            Socket = new WebSocketShim(socket, msPool);
            LoadOperationHandlers();
        }

        public void Dispose()
        {
            Socket?.Dispose();
        }

        protected abstract Task<bool> Handshake(CancellationToken cancellationToken);

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (!await Handshake(cancellationToken))
                return;

            try
            {
                while (true)
                {
                    using var dsContext = await Socket.ReceiveObjectAsync(cancellationToken);
                    var message = await dsContext.DeserializeAsync<Message>(cancellationToken);

                    // unsupported message
                    if (!_handlers.TryGetValue(message.OperationCode, out var handler))
                        throw new WebSocketControllerException(4003, "Unsupported Operation Code");

                    // lookup handler
                    var (callee, attribute ) = handler;
                    var param = await dsContext.DeserializeAsync(attribute.MessageType, cancellationToken);
                    await callee.Invoke(new WebSocketOperationHandlerContext((Message) param, Socket));

                    // session cancellation
                    if (cancellationToken.IsCancellationRequested)
                        throw new WebSocketControllerException(1012, "Server Application is Shutting Down");
                }
            }
            catch (WebSocketControllerException e)
            {
                var token = cancellationToken.IsCancellationRequested ? cancellationToken : CancellationToken.None;
                await CloseAsync(e.Code, e.Reason, token);
            }
        }

        private async Task CloseAsync(int code, string message, CancellationToken cancellationToken)
        {
            await Socket.SendObjectAsync(new ConnectionClosed(code, message), CancellationToken.None);
            if (!Enum.TryParse<WebSocketCloseStatus>(code.ToString(), out var closeStatus))
                closeStatus = WebSocketCloseStatus.ProtocolError;
            await Socket.EnsureClosedAsync(closeStatus, message, CancellationToken.None);
        }

        private void LoadOperationHandlers()
        {
            var candidates = GetType().GetMethods(BindingFlags.Instance);
            foreach (var method in candidates)
            {
                var attribute = method.GetCustomAttribute<WebSocketOperationHandlerAttribute>();
                if (attribute == null)
                    continue;

                var callee = method.CreateDelegate(typeof(WebSocketOperationHandler), this);
                _handlers.Add(attribute.OperationCode, ((WebSocketOperationHandler) callee, attribute));
            }
        }
    }
}