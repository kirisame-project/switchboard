using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
using Switchboard.Controllers.WebSocketized.Abstractions;
using Switchboard.Controllers.WebSocketized.Contracts;
using Switchboard.Controllers.WebSocketized.Contracts.Common;
using Switchboard.Controllers.WebSocketsX.Facilities.Attributes;
using Switchboard.Controllers.WebSocketsX.Facilities.Exceptions;

namespace Switchboard.Controllers.WebSocketsX.Facilities
{
    internal abstract class WebSocketController : IWebSocketController, IDisposable
    {
        private static readonly IDictionary<Type, IList<(MethodInfo, OperationHandlerAttribute)>> HandlerCache;

        private readonly RecyclableMemoryStreamManager _memoryStreamManager;

        private readonly IDictionary<int, (OperationHandler, OperationHandlerAttribute)> _operationHandlers;

        protected WebSocketShim Socket;

        static WebSocketController()
        {
            HandlerCache = new ConcurrentDictionary<Type, IList<(MethodInfo, OperationHandlerAttribute)>>();
        }

        protected WebSocketController(RecyclableMemoryStreamManager memoryStreamManager)
        {
            _memoryStreamManager = memoryStreamManager;
            _operationHandlers = new ConcurrentDictionary<int, (OperationHandler, OperationHandlerAttribute)>(
                GetOperationHandlers()
            );

            foreach (var value in Enum.GetValues(typeof(OperationCodes)))
                Debug.Assert(_operationHandlers.ContainsKey((int) value),
                    $"Missing handler for operation code {value} ({(int) value})",
                    "No handler is implemented for such operation code, or cannot be found by reflection");
        }

        public void Dispose()
        {
            Socket?.Dispose();
        }

        public virtual async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            Socket = new WebSocketShim(socket, _memoryStreamManager);
            await RunAsync(cancellationToken);

            // TODO: exception handling
        }

        protected virtual async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var stream = await Socket.ReceiveStreamAsync(cancellationToken);
                    Message message;

                    // deserialize base message for operation code
                    try
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        message = await JsonSerializer.DeserializeAsync<Message>(stream,
                            cancellationToken: cancellationToken);
                    }
                    catch (JsonException)
                    {
                        throw new WebSocketControllerException(4404, "Deserialization Failure");
                    }

                    // lookup handler for operation code
                    if (!_operationHandlers.TryGetValue(message.OperationCode, out var handler))
                        throw new WebSocketControllerException(4403, "Unsupported Operation Code");
                    var (callee, attribute) = handler;

                    // deserialize for passing to handlers
                    try
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        message = (Message) await JsonSerializer.DeserializeAsync(stream, attribute.MessageType,
                            cancellationToken: cancellationToken);
                    }
                    catch (JsonException)
                    {
                        throw new WebSocketControllerException(4404, "Deserialization Failure");
                    }

                    // invoke operation handler
                    var param = new OperationHandlerContext(message);
                    try
                    {
                        await callee.Invoke(param, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        // hide unexpected errors in production

                        if (Debugger.IsAttached) throw;

                        throw e switch
                        {
                            NotImplementedException _ =>
                            new WebSocketControllerException(4501, "Unexpected Not Implemented Code Path"),

                            _ =>
                            new WebSocketControllerException(4500, "Generic Internal Server Error")
                        };
                    }
                }
            }
            catch (WebSocketControllerException e)
            {
                await Socket.SendObjectAsync(new CloseSession(e.ClosureCode, e.ClosureReason), cancellationToken);
                await Socket.EnsureClosedAsync(e.WebSocketCloseStatus, e.ClosureReason, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        private IEnumerable<KeyValuePair<int, (OperationHandler, OperationHandlerAttribute)>> GetOperationHandlers()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var currentType = GetType();
            if (!HandlerCache.TryGetValue(currentType, out var methods))
            {
                methods = new List<(MethodInfo, OperationHandlerAttribute)>(
                    from method in currentType.GetMethods(flags)
                    let attribute = method.GetCustomAttribute<OperationHandlerAttribute>()
                    where attribute != null
                    select (method, attribute));
                HandlerCache.Add(currentType, methods);
            }

            foreach (var (method, attribute) in methods)
            {
                var callee = method.CreateDelegate(typeof(OperationHandler), this);
                yield return new KeyValuePair<int, (OperationHandler, OperationHandlerAttribute)>(
                    attribute.OperationCode, ((OperationHandler) callee, attribute)
                );
            }
        }
    }
}