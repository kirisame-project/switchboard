namespace Switchboard.Metrics
{
    public class MetricsConfiguration
    {
        public int FlushInterval { get; set; }

        public InfluxDbConfiguration InfluxDb { get; set; }
    }
}