using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Switchboard.Services.Lambda
{
    public class VectorSubTask : SubTaskBase
    {
        [JsonIgnore]
        public IDictionary<Guid, Stream> FaceImages { get; set; } = new ConcurrentDictionary<Guid, Stream>();
    }
}