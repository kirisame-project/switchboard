using System;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Switchboard.Common;
using Switchboard.Services.Upstream;

namespace Switchboard.Services.FaceReporting
{
    [Component(ComponentLifestyle.Singleton, Implements = typeof(IReportingService))]
    internal class HttpReportingService : IReportingService
    {
        private readonly HttpReportingConfiguration _config;

        private readonly HttpClient _httpClient;

        private readonly ILogger _logger;

        private HttpStatusCode _lastEndpointError = HttpStatusCode.OK;

        public HttpReportingService(HttpReportingConfiguration config, HttpClient httpClient, ILoggerFactory logger)
        {
            _config = config;
            _httpClient = httpClient;
            _logger = logger.CreateLogger(GetType());
        }

        public async Task ReportLabelAsync(int label, double distance, CancellationToken cancellationToken)
        {
            if (distance < _config.LabelReportingThreshold)
                return;

            var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(_config.Timeout)).Token;
            var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken).Token;

            var payload = new JsonObjectContent<ReportPayload>(new ReportPayload(label, distance, _config.AccessToken));
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(_config.LabelReportingEndpoint, payload, token);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return;
            }

            if (response.IsSuccessStatusCode || response.StatusCode == _lastEndpointError)
                return;

            _logger.LogError($"Label Reporting POST failed: {(int) response.StatusCode} {response.ReasonPhrase}");
            _lastEndpointError = response.StatusCode;
        }

        private class ReportPayload
        {
            public ReportPayload(int label, double distance, string accessToken)
            {
                Label = label;
                Distance = distance;
                AccessToken = accessToken;
                RequestId = Guid.NewGuid();
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            [JsonPropertyName("_requestId")] public Guid RequestId { get; }

            [JsonPropertyName("distance")] public double Distance { get; }

            [JsonPropertyName("label")] public int Label { get; }

            [JsonPropertyName("token")] public string AccessToken { get; }

            [JsonPropertyName("timestamp")] public long Timestamp { get; }
        }
    }
}