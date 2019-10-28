using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Common;

namespace Switchboard.Services.Upstream
{
    [Component]
    public class HttpHelper
    {
        private readonly HttpClient _client;

        public HttpHelper(HttpClient client)
        {
            _client = client;
        }

        private async Task PostStreamAsync(MemoryStream content, string contentType, string url, Stream resultBuffer,
            CancellationToken token)
        {
            content.Seek(0, SeekOrigin.Begin);
            // TODO: JESUS, CAN WE MAKE LESS MEMORY COPY?
            using var payload = new ByteArrayContent(content.ToArray());
            payload.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

            using var request = await _client.PostAsync(url, payload, token);
            request.EnsureSuccessStatusCode();
            await request.Content.CopyToAsync(resultBuffer);
        }

        public async Task<T> PostStreamAsync<T>(MemoryStream content, string contentType, string url,
            CancellationToken token)
        {
            await using var result = new MemoryStream();
            await PostStreamAsync(content, contentType, url, result, token);

            result.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(result).ReadToEndAsync();

            result.Seek(0, SeekOrigin.Begin);
            return await JsonSerializer.DeserializeAsync<T>(result, cancellationToken: token);
        }

        public async Task<TResult> PostObjectAsync<TResult, TObject>(TObject obj, string url, CancellationToken token)
        {
            await using var buffer = new MemoryStream();
            await JsonSerializer.SerializeAsync(buffer, obj, typeof(TObject), cancellationToken: token);

            buffer.Seek(0, SeekOrigin.Begin);
            return await PostStreamAsync<TResult>(buffer, "application/json", url, token);
        }
    }
}