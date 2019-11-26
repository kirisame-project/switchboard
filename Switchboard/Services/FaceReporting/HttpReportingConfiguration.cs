namespace Switchboard.Services.FaceReporting
{
    internal class HttpReportingConfiguration
    {
        public string AccessToken { get; set; }

        public string LabelReportingEndpoint { get; set; }

        public double LabelReportingThreshold { get; set; }

        public int Timeout { get; set; }
    }
}