using System.Text.Json.Serialization;

namespace Switchboard.Services.Common.Contracts
{
    /// <summary>
    ///     Represents a matching face with the label "Label" and the similarity "Distance"
    /// </summary>
    internal class FaceSearchResult
    {
        [JsonPropertyName("distance")] public double Distance { get; set; }

        [JsonPropertyName("label")] public int Label { get; set; }
    }
}