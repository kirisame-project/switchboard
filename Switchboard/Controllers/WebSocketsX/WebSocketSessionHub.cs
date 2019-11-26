using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
using Switchboard.Common;
using Switchboard.Controllers.WebSocketized.Abstractions;
using Switchboard.Services.FaceRecognition;

namespace Switchboard.Controllers.WebSocketsX
{
    [Component(ComponentLifestyle.Singleton, Implements = typeof(IWebSocketSessionHub))]
    internal class WebSocketSessionHub : IWebSocketSessionHub
    {
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;

        private readonly IDictionary<Guid, IWebSocketSession> _sessions;

        private readonly RecognitionTaskFactory _taskFactory;

        private readonly IRecognitionTaskRunner _taskRunner;

        public WebSocketSessionHub(IRecognitionTaskRunner taskRunner, RecognitionTaskFactory taskFactory,
            RecyclableMemoryStreamManager memoryStreamManager)
        {
            _taskRunner = taskRunner;
            _taskFactory = taskFactory;
            _memoryStreamManager = memoryStreamManager;
            _sessions = new ConcurrentDictionary<Guid, IWebSocketSession>();
        }

        public async Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            using var session = new WebSocketSession(_taskRunner, _taskFactory, _memoryStreamManager);
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