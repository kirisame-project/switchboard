using System;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Services.FaceRecognition;

namespace Switchboard.Controllers.WebSocketized.Abstractions
{
    internal interface IWebSocketSession
    {
        Guid SessionId { get; }

        WebSocketSessionState SessionState { get; }

        Task SendTaskUpdateAsync(RecognitionTask task, CancellationToken cancellationToken);
    }
}