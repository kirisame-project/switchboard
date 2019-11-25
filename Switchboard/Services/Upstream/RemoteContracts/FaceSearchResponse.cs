using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Switchboard.Services.Upstream.RemoteContracts
{
    internal class FaceSearchResponse
    {
        [JsonPropertyName("code")] public int Code { get; set; }

        [JsonPropertyName("result")] public IDictionary<string, SearchResultSet> ResultSet { get; set; }

        [JsonPropertyName("time_used")] public int Time { get; set; }

        [JsonPropertyName("error_message")] public string ErrorMessage { get; set; }

        public class SearchResultSet
        {
            [JsonPropertyName("distance")] public double[] TopDistances { get; set; }

            [JsonPropertyName("ids")] public int[] TopLabels { get; set; }
        }
    }
}