using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Common;

namespace Switchboard.Controllers.WebSocketized
{
    [Component(ComponentLifestyle.Singleton)]
    public class WebSocketController : IDisposable, IWebSocketController
    {
        private const int BufferSize = 1024 * 1024;

        private const int MaxBufferCount = 16;

        private readonly WebSocketBufferPool _bufferPool;

        private readonly WebSocketSessionConfiguration _sessionConfig;

        private readonly IDictionary<Guid, IWebSocketSession> _sessions;

        public WebSocketController(WebSocketSessionConfiguration sessionConfig)
        {
            _sessionConfig = sessionConfig;
            _bufferPool = new WebSocketBufferPool(BufferSize, MaxBufferCount);
            _sessions = new ConcurrentDictionary<Guid, IWebSocketSession>();
        }

        public void Dispose()
        {
            foreach (var (_, session) in _sessions)
                session.Dispose();
        }

        public async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            var session = new WebSocketSession(socket, _bufferPool, _sessionConfig);

            var sessionId = session.SessionId;
            session.OnClose += () => _sessions.Remove(sessionId);
            _sessions.Add(sessionId, session);

            try
            {
                await session.RunAsync(cancellationToken);
            }
            finally
            {
                session.Dispose();
            }
        }

        public bool TryGetSession(Guid sessionId, out IWebSocketSession session)
        {
            return _sessions.TryGetValue(sessionId, out session);
        }
    }
}