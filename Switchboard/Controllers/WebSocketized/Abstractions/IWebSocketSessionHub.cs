using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Services.FaceRecognition;

namespace Switchboard.Controllers.WebSocketized.Abstractions
{
    internal interface IWebSocketSessionHub
    {
        Task AcceptAsync(WebSocket socket, CancellationToken cancellationToken);

        Task BroadcastTaskUpdateAsync(RecognitionTask task, CancellationToken cancellationToken);

        bool TryGetSession(Guid sessionId, out IWebSocketSession session);
    }
}