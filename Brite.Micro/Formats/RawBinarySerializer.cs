using System;
using System.Threading.Tasks;
using Brite.Utility.IO;
using MemoryStream = Brite.Utility.IO.MemoryStream;

namespace Brite.Micro.Formats
{
    public class RawBinarySerializer : IBinarySerializer
    {
        public async Task<MemoryStream> SerializeAsync(IStream stream)
        {
            var outputStream = new MemoryStream();

            byte[] buffer = new byte[1];
            while (true)
            {
                try
                {
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                    await outputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (TimeoutException)
                {
                    break;
                }
            }

            return outputStream;
        }

        public async Task<MemoryStream> DeserializeAsync(IStream stream)
        {
            var outputStream = new MemoryStream();

            byte[] buffer = new byte[1];
            while (true)
            {
                try
                {
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                    await outputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (TimeoutException)
                {
                    break;
                }
            }

            return outputStream;
        }
    }
}
