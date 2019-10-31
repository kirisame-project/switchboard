using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Switchboard.Services.Upstream
{
    public class JsonObjectContent<T> : HttpContent
    {
        private readonly T _obj;

        public JsonObjectContent(T obj)
        {
            _obj = obj;
            Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            await JsonSerializer.SerializeAsync(stream, _obj);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}