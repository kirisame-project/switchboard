namespace Switchboard.Metrics
{
    internal class MetricsConfiguration
    {
        public int FlushInterval { get; set; }

        public InfluxDbConfiguration InfluxDb { get; set; }
    }
}