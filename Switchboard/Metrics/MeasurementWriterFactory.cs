using System.Collections.Generic;
using Switchboard.Metrics.Collector;

namespace Switchboard.Metrics
{
    public class MeasurementWriterFactory
    {
        private readonly IMetricsCollector _collector;

        private readonly IReadOnlyDictionary<string, string> _predefinedTags;

        public MeasurementWriterFactory(IReadOnlyDictionary<string, string> predefinedTags, IMetricsCollector collector)
        {
            _predefinedTags = predefinedTags;
            _collector = collector;
        }

        public MeasurementWriter GetInstance(IReadOnlyDictionary<string, string> predefinedTags)
        {
            var tags = new Dictionary<string, string>(_predefinedTags);
            foreach (var (key, value) in predefinedTags)
                tags.Add(key, value);
            return new MeasurementWriter(_collector, tags);
        }
    }
}