using System;
using System.Text.Json.Serialization;

namespace Switchboard.Controllers.WebSocketized.Contracts.Remote
{
    internal class ImageTaskRequest : MessageWithPayload<ImageTaskRequest.Content>
    {
        [Obsolete("For deserialization only")]
        public ImageTaskRequest() : base(0, null)
        {
        }

        internal class Content
        {
            [JsonPropertyName("contentLength")] public int ContentLength { get; set; }

            [JsonPropertyName("contentType")] public string ContentType { get; set; }
        }
    }
}