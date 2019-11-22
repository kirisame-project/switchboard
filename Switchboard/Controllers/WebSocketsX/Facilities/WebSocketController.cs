using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Controllers.WebSocketized.Abstractions;
using Switchboard.Controllers.WebSocketized.Contracts;
using Switchboard.Controllers.WebSocketsX.Facilities.Attributes;
using Switchboard.Controllers.WebSocketsX.Facilities.Buffers;

namespace Switchboard.Controllers.WebSocketsX.Facilities
{
    internal abstract class WebSocketController : IWebSocketController, IDisposable
    {
        private readonly MemoryStreamPool _memoryStreamPool;

        private readonly IDictionary<int, (OperationHandler, OperationHandlerAttribute)> _operationHandlers;

        protected WebSocketShim Socket;

        protected WebSocketController(MemoryStreamPool memoryStreamPool)
        {
            _memoryStreamPool = memoryStreamPool;
            _operationHandlers = new ConcurrentDictionary<int, (OperationHandler, OperationHandlerAttribute)>(
                GetOperationHandlers()
            );
        }

        public void Dispose()
        {
            Socket?.Dispose();
        }

        public async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            Socket = new WebSocketShim(socket, _memoryStreamPool);
            await RunAsync(cancellationToken);
            // TODO: exception handling
        }

        protected virtual async Task RunAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested)
            {
                var ctx = await Socket.ReceiveObjectAsync(cancellationToken);
                var message = await ctx.DeserializeAsync<Message>(cancellationToken);

                _operationHandlers.TryGetValue(message.OperationCode, out var handler);
                var (callee, attribute) = handler;

                message = (Message) await ctx.DeserializeAsync(attribute.MessageType, cancellationToken);
                var param = new OperationHandlerContext(message);

                await callee.Invoke(param, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        private IEnumerable<KeyValuePair<int, (OperationHandler, OperationHandlerAttribute)>> GetOperationHandlers()
        {
            foreach (var method in GetType().GetMethods())
            {
                var attribute = method.GetCustomAttribute<OperationHandlerAttribute>();
                if (attribute == null)
                    continue;

                var callee = method.CreateDelegate(typeof(OperationHandler), this);
                yield return new KeyValuePair<int, (OperationHandler, OperationHandlerAttribute)>(
                    attribute.OperationCode, ((OperationHandler) callee, attribute)
                );
            }

            foreach (var value in Enum.GetValues(typeof(OperationCodes)))
                Debug.Assert(_operationHandlers.ContainsKey((int) value),
                    $"Missing handler for operation code {value} ({(int) value})",
                    "No handler is implemented for such operation code, or cannot be found by reflection");
        }
    }
}