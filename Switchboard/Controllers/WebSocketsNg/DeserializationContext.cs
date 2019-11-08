using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Switchboard.Controllers.WebSocketsNg
{
    internal class DeserializationContext : RentedObjectHolder<MemoryStream>
    {
        private readonly MemoryStream _stream;

        public DeserializationContext(MemoryStream stream, IObjectOwner<MemoryStream> owner) : base(stream, owner)
        {
            _stream = stream;
        }

        public async Task<T> DeserializeAsync<T>(CancellationToken cancellationToken)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            return await JsonSerializer.DeserializeAsync<T>(_stream, cancellationToken: cancellationToken);
        }
    }
}