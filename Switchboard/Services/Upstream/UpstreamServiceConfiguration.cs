using Switchboard.Common;

namespace Switchboard.Services.Upstream
{
    [Component]
    public class UpstreamServiceConfiguration
    {
        public UpstreamServiceEndpointConfiguration Endpoints { get; set; }

        public class UpstreamServiceEndpointConfiguration
        {
            public string Detection { get; set; }

            public string DetectionV2 { get; set; }

            public string Recognition { get; set; }

            public string Search { get; set; }
        }
    }
}