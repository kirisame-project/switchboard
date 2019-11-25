using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;

namespace Switchboard.Metrics.Collector
{
    internal class QueuedMetricsCollector : IMetricsCollector
    {
        private readonly LineProtocolClient _client;

        private readonly ConcurrentQueue<LineProtocolPoint> _queue;

        public QueuedMetricsCollector(LineProtocolClient client)
        {
            _client = client;
            _queue = new ConcurrentQueue<LineProtocolPoint>();
        }

        public void Write(string measurement, IReadOnlyDictionary<string, object> fields)
        {
            Write(measurement, fields, null);
        }

        public void Write(string measurement, IReadOnlyDictionary<string, object> fields,
            IReadOnlyDictionary<string, string> tags)
        {
            _queue.Enqueue(new LineProtocolPoint(measurement, fields, tags, DateTime.UtcNow));
        }

        public async Task SendBatchAsync(CancellationToken cancellationToken)
        {
            if (_queue.Count == 0)
                return;

            var payload = new LineProtocolPayload();
            while (_queue.TryDequeue(out var point))
                payload.Add(point);

            var result = await _client.WriteAsync(payload, cancellationToken);
            if (!result.Success)
                throw new Exception(result.ErrorMessage);
        }
    }
}