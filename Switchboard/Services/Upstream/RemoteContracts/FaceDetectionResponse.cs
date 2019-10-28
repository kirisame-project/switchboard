﻿using System.Text.Json.Serialization;

namespace Switchboard.Services.Upstream.RemoteContracts
{
    public class FaceDetectionResponse
    {
        [JsonPropertyName("code")] public string Code { get; set; }

        [JsonPropertyName("box")] public int[] Box { get; set; }
    }
}