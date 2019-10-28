﻿using System.Text.Json.Serialization;

namespace Switchboard.Services.Upstream.RemoteContracts
{
    public class FaceDetectionV2Response
    {
        [JsonPropertyName("count")] public int Count { get; set; }

        [JsonPropertyName("boxes")] public int[][] Boxes { get; set; }
    }
}