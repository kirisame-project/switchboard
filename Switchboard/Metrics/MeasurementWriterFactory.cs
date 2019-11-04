using System.Collections.Generic;
using InfluxDB.Collector;

namespace Switchboard.Metrics
{
    public class MeasurementWriterFactory
    {
        private readonly MetricsCollector _collector;

        private readonly IReadOnlyDictionary<string, object> _predefinedTags;

        public MeasurementWriterFactory(IReadOnlyDictionary<string, object> predefinedTags, MetricsCollector collector)
        {
            _predefinedTags = predefinedTags;
            _collector = collector;
        }

        public MeasurementWriter GetInstance(IReadOnlyDictionary<string, object> predefinedTags)
        {
            var tags = new Dictionary<string, object>(_predefinedTags);
            foreach (var (key, value) in predefinedTags)
                tags.Add(key, value);
            return new MeasurementWriter(_collector, tags);
        }
    }
}