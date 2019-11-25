using System.Collections.Generic;

namespace Switchboard.Metrics.Collector
{
    internal interface IMetricsCollector
    {
        void Write(string measurement, IReadOnlyDictionary<string, object> fields);

        void Write(string measurement, IReadOnlyDictionary<string, object> fields,
            IReadOnlyDictionary<string, string> tags);
    }
}