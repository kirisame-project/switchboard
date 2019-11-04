using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Services.Upstream
{
    public static class HttpClientExtensions
    {
        private static async Task<Stream> PostAsync(this HttpClient client, HttpContent content, string url,
            CancellationToken cancellationToken)
        {
            using var request = await client.PostAsync(url, content, cancellationToken);
            request.EnsureSuccessStatusCode();

            var result = new MemoryStream();
            await request.Content.CopyToAsync(result);
            return result;
        }

        private static async Task<Stream> PostStreamAsync(this HttpClient client, Stream stream, string contentType,
            string url,
            CancellationToken cancellationToken)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using var content = new CopyStreamContent(stream);
            content.Headers.ContentLength = stream.Length;
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            return await client.PostAsync(content, url, cancellationToken);
        }

        public static async Task<T> PostStreamAsync<T>(this HttpClient client, Stream stream, string contentType,
            string url, CancellationToken cancellationToken)
        {
            await using var result = await client.PostStreamAsync(stream, contentType, url, cancellationToken);
            result.Seek(0, SeekOrigin.Begin);
            return await JsonSerializer.DeserializeAsync<T>(result, cancellationToken: cancellationToken);
        }

        private static async Task<Stream> PostObjectAsync<T>(this HttpClient client, T obj, string url,
            CancellationToken cancellationToken)
        {
            using var content = new JsonObjectContent<T>(obj);
            return await client.PostAsync(content, url, cancellationToken);
        }

        public static async Task<TResult> PostObjectAsync<TResult, TObject>(this HttpClient client, TObject obj,
            string url, CancellationToken cancellationToken)
        {
            await using var stream = await client.PostObjectAsync(obj, url, cancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            return await JsonSerializer.DeserializeAsync<TResult>(stream, cancellationToken: cancellationToken);
        }
    }
}