using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Switchboard.Controllers.WebSocketsX.Facilities.Buffers;

namespace Switchboard.Controllers.WebSocketsX.Facilities
{
    internal class DeserializationContext : ObjectHolder<MemoryStream>
    {
        public DeserializationContext(MemoryStream stream, IObjectOwner<MemoryStream> owner) : base(stream, owner)
        {
        }

        public async Task<T> DeserializeAsync<T>(CancellationToken cancellationToken)
        {
            return (T) await DeserializeAsync(typeof(T), cancellationToken);
        }

        public async Task<object> DeserializeAsync(Type type, CancellationToken cancellationToken)
        {
            Obj.Seek(0, SeekOrigin.Begin);
            return await JsonSerializer.DeserializeAsync(Obj, type, cancellationToken: cancellationToken);
        }
    }
}