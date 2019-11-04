using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Collector;

namespace Switchboard.Metrics
{
    public class MeasurementWriter
    {
        private readonly MetricsCollector _collector;
        private readonly IReadOnlyDictionary<string, string> _predefinedTags;

        public MeasurementWriter(MetricsCollector collector, IDictionary<string, object> predefinedPredefinedTags)
        {
            _collector = collector;
            _predefinedTags = predefinedPredefinedTags.ToDictionary(p => p.Key, p => p.Value.ToString());
        }

        public void Measure(MeasurementOptions measurement, object value)
        {
            _collector.Measure(measurement.Name, value, _predefinedTags);
        }

        [Obsolete]
        private void Write(MeasurementOptions measurement, IReadOnlyDictionary<string, object> fields)
        {
            _collector.Write(measurement.Name, fields, _predefinedTags);
        }

        [Obsolete]
        private void Write(MeasurementOptions measurement, string fieldName, object value)
        {
            Write(measurement, new Dictionary<string, object> {{fieldName, value}});
        }

        [Obsolete]
        public void Write(MeasurementOptions measurement, object value)
        {
            Write(measurement, measurement.DefaultFieldName, value);
        }
    }
}