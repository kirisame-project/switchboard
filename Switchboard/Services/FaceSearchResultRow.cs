using System.Text.Json.Serialization;

namespace Switchboard.Services
{
    /// <summary>
    ///     Represents a matching face with the label "Label" and the similarity "Distance"
    /// </summary>
    public class FaceSearchResultRow
    {
        [JsonPropertyName("distance")] public double Distance { get; set; }

        [JsonPropertyName("label")] public int Label { get; set; }
    }
}