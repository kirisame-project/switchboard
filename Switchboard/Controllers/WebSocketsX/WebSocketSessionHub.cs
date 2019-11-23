using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Common;
using Switchboard.Controllers.WebSocketized.Abstractions;
using Switchboard.Controllers.WebSocketsX.Facilities.Buffers;
using Switchboard.Services.FaceRecognition;

namespace Switchboard.Controllers.WebSocketsX
{
    [Component(ComponentLifestyle.Singleton, Implements = typeof(IWebSocketSessionHub))]
    internal class WebSocketSessionHub : IWebSocketSessionHub
    {
        private readonly MemoryStreamPool _memoryStreamPool;

        private readonly IFaceRecognitionService _recognitionService;

        private readonly IDictionary<Guid, IWebSocketSession> _sessions;

        public WebSocketSessionHub(IFaceRecognitionService recognitionService, MemoryStreamPool memoryStreamPool)
        {
            _recognitionService = recognitionService;
            _memoryStreamPool = memoryStreamPool;
            _sessions = new ConcurrentDictionary<Guid, IWebSocketSession>();
        }

        public async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            using var session = new WebSocketSession(_recognitionService, _memoryStreamPool);
            _sessions.Add(session.SessionId, session);
            try
            {
                await session.AcceptAsync(socket, cancellationToken);
            }
            finally
            {
                _sessions.Remove(session.SessionId);
            }
        }

        public async Task BroadcastTaskUpdateAsync(RecognitionTask task, CancellationToken cancellationToken)
        {
            foreach (var (_, session ) in _sessions) await session.SendTaskUpdateAsync(task, cancellationToken);
        }

        public bool TryGetSession(Guid sessionId, out IWebSocketSession session)
        {
            return _sessions.TryGetValue(sessionId, out session);
        }
    }
}