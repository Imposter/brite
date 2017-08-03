using System.Threading.Tasks;
using MStream = System.IO.MemoryStream;

namespace Brite.Utility.IO
{
    public class MemoryStream : IStream
    {
        private readonly MStream _stream;

        public MemoryStream()
        {
            _stream = new MStream();
        }

        public MemoryStream(byte[] buffer)
        {
            _stream = new MStream(buffer);
        }

        public MemoryStream(byte[] buffer, int offset, int length)
        {
            _stream = new MStream(buffer, offset, length);
        }

        public async Task<int> ReadAsync(byte[] buffer, int offset, int length)
        {
            return await _stream.ReadAsync(buffer, offset, length);
        }

        public async Task WriteAsync(byte[] buffer, int offset, int length)
        {
            await _stream.WriteAsync(buffer, offset, length);
        }

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
