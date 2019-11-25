using System.Text.Json.Serialization;

namespace Switchboard.Services
{
    public struct FacePosition
    {
        [JsonPropertyName("x1")] public int X1 { get; set; }

        [JsonPropertyName("x2")] public int X2 { get; set; }

        [JsonPropertyName("y1")] public int Y1 { get; set; }

        [JsonPropertyName("y2")] public int Y2 { get; set; }
    }
}