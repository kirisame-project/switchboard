using System;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Services.Lambda;

namespace Switchboard.Controllers.WebSocketized
{
    public interface IWebSocketSession : IDisposable
    {
        Guid SessionId { get; }

        bool SessionActive { get; }

        Task RunAsync(CancellationToken cancellationToken);

        Task SendTaskUpdateAsync(LambdaTask task, CancellationToken cancellationToken);
    }
}