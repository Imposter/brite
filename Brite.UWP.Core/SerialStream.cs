using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Brite.Utility.IO;

namespace Brite.UWP.Core
{
    public class SerialStream : IStream
    {
        private readonly DataReader _reader;
        private readonly DataWriter _writer;

        public SerialStream(IInputStream input, IOutputStream output)
        {
            _reader = new DataReader(input);
            _writer = new DataWriter(output);
        }

        public async Task<int> ReadAsync(byte[] buffer, int offset, int length)
        {
            var readCount = await _reader.LoadAsync((uint)length);
            if (offset > 0 || readCount < length)
            {
                byte[] tempBuffer = new byte[readCount];
                _reader.ReadBytes(tempBuffer);
                tempBuffer.CopyTo(buffer, offset);
            }
            else
            {
                _reader.ReadBytes(buffer);
            }

            return (int)readCount;
        }

        public async Task WriteAsync(byte[] buffer, int offset, int length)
        {
            _writer.WriteBytes(offset > 0 ? buffer.Skip(offset).ToArray() : buffer);
            await _writer.StoreAsync();
        }
    }
}