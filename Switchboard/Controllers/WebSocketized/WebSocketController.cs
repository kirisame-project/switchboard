using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using Switchboard.Common;

namespace Switchboard.Controllers.WebSocketized
{
    [Component(ComponentLifestyle.Singleton)]
    public class WebSocketController
    {
        private const int BufferSize = 1024 * 1024;

        private readonly ObjectPool<byte[]> _bufferPool;
        private readonly WebSocketSessionConfiguration _sessionConfig;

        private readonly IDictionary<Guid, WebSocketSession> _sessions;

        public WebSocketController(WebSocketSessionConfiguration sessionConfig)
        {
            _sessionConfig = sessionConfig;
            _bufferPool = new DefaultObjectPool<byte[]>(new EasyObjectPoolPolicy<byte[]>(() => new byte[BufferSize]));
            _sessions = new ConcurrentDictionary<Guid, WebSocketSession>();
        }

        public async Task Accept(WebSocket socket, CancellationToken cancellationToken)
        {
            var session = new WebSocketSession(socket, _bufferPool, _sessionConfig);
            session.OnClose += () => _sessions.Remove(session.SessionId);
            _sessions.Add(session.SessionId, session);
            await session.Run(cancellationToken);
        }

        public bool TryGetSession(Guid sessionId, out WebSocketSession session)
        {
            return _sessions.TryGetValue(sessionId, out session);
        }

        public bool ContainsSession(Guid sessionId)
        {
            return _sessions.TryGetValue(sessionId, out var session) && session.IsSessionActive();
        }
    }
}
