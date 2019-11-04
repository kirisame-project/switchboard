namespace Switchboard
{
    public class MetricsConfiguration
    {
        public class InfluxDbConfiguration
        {
            public string BaseUri { get; set; }

            public string Database { get; set; }

            public string Password { get; set; }

            public string Username { get; set; }
        }
        
        public int FlushInterval { get; set; }

        public InfluxDbConfiguration InfluxDb { get; set; }
    }
}