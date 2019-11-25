namespace Switchboard.Metrics
{
    internal class InfluxDbConfiguration
    {
        public string BaseUri { get; set; }

        public string Database { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }
    }
}