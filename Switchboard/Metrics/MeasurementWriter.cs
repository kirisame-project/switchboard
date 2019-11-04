using System.Collections.Generic;
using System.Linq;
using Switchboard.Metrics.Collector;

namespace Switchboard.Metrics
{
    public class MeasurementWriter
    {
        private readonly IMetricsCollector _collector;
        private readonly IReadOnlyDictionary<string, string> _predefinedTags;

        public MeasurementWriter(IMetricsCollector collector, IDictionary<string, string> predefinedPredefinedTags)
        {
            _collector = collector;
            _predefinedTags = predefinedPredefinedTags.ToDictionary(p => p.Key, p => p.Value.ToString());
        }

        private void Write(MeasurementOptions measurement, IReadOnlyDictionary<string, object> fields)
        {
            _collector.Write(measurement.Name, fields, _predefinedTags);
        }

        private void Write(MeasurementOptions measurement, string fieldName, object value)
        {
            Write(measurement, new Dictionary<string, object> {{fieldName, value}});
        }

        public void Write(MeasurementOptions measurement, object value)
        {
            Write(measurement, measurement.DefaultFieldName, value);
        }
    }
}