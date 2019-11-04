using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Switchboard.Services.Upstream
{
    public class CopyStreamContent : HttpContent
    {
        private readonly Stream _stream;

        public CopyStreamContent(Stream stream)
        {
            _stream = stream;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            await _stream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _stream.Length;
            return true;
        }
    }
}