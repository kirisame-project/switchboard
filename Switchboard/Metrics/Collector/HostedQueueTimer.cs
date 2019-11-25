using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Switchboard.Metrics.Collector
{
    internal class HostedQueueTimer : IHostedService, IDisposable
    {
        private readonly QueuedMetricsCollector _collector;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly TimeSpan _interval;

        private Timer _timer;

        public HostedQueueTimer(QueuedMetricsCollector collector, TimeSpan interval)
        {
            _collector = collector;
            _interval = interval;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(_ =>
            {
                _collector.SendBatchAsync(_cts.Token).ContinueWith(task =>
                {
                    if (!task.IsCompletedSuccessfully)
                        OnError?.Invoke(task.Exception);
                }, _cts.Token);
            }, null, TimeSpan.Zero, _interval);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, 0);
            _cts.Cancel();
            return Task.CompletedTask;
        }

        public event Action<Exception> OnError;
    }
}