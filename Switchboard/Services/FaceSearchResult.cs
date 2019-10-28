using System.Text.Json.Serialization;

namespace Switchboard.Services
{
    /// <summary>
    ///     Represents the result for one single face search with top matched faces
    /// </summary>
    public class FaceSearchResult
    {
        [JsonPropertyName("top_results")] public FaceSearchResultRow[] TopResults { get; set; }
    }
}