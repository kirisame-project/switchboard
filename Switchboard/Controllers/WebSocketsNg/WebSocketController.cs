using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Common;
using Switchboard.Controllers.WebSocketized;

namespace Switchboard.Controllers.WebSocketsNg
{
    [Component(ComponentLifestyle.Singleton, Implements = typeof(IWebSocketController))]
    public class WebSocketController : IWebSocketController
    {
        private readonly MemoryStreamPool _memoryStreamPool;

        private readonly IDictionary<Guid, IWebSocketSession> _sessions;

        public WebSocketController(MemoryStreamPool memoryStreamPool)
        {
            _memoryStreamPool = memoryStreamPool;
            _sessions = new ConcurrentDictionary<Guid, IWebSocketSession>();
        }

        public async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            using var session = new WebSocketSession(socket, _memoryStreamPool);
            try
            {
                _sessions.Add(session.SessionId, session);
                await session.RunAsync(cancellationToken);
            }
            finally
            {
                _sessions.Remove(session.SessionId);
            }
        }

        public bool TryGetSession(Guid sessionId, out IWebSocketSession session)
        {
            return _sessions.TryGetValue(sessionId, out session);
        }
    }
}
