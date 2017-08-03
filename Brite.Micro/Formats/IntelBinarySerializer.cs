using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Brite.Utility.IO;
using MemoryStream = Brite.Utility.IO.MemoryStream;

namespace Brite.Micro.Formats
{
    // For more information, see: http://www.interlog.com/~speff/usefulinfo/Hexfrmt.pdf
    public class IntelBinarySerializer : IBinarySerializer
    {
        // TODO: Implement
        public Task<MemoryStream> SerializeAsync(IStream stream)
        {
            throw new NotImplementedException();
        }

        public async Task<MemoryStream> DeserializeAsync(IStream stream)
        {
            var inputStream = new BinaryStream(stream);
            var outputStream = new MemoryStream();

            var newlineCharacters = Environment.NewLine.ToCharArray();

            while (true)
            {
                char recordMark;
                try
                {
                    recordMark = await inputStream.ReadCharAsync();
                }
                catch (TimeoutException)
                {
                    break;
                }

                if (newlineCharacters.Contains(recordMark))
                    continue;

                if (recordMark != ':')
                    throw new InvalidDataException("Invalid record mark");

                var recordLength = byte.Parse(await inputStream.ReadStringAsync(2), NumberStyles.AllowHexSpecifier);
                var recordOffset = ushort.Parse(await inputStream.ReadStringAsync(4), NumberStyles.AllowHexSpecifier);
                var recordType = byte.Parse(await inputStream.ReadStringAsync(2), NumberStyles.AllowHexSpecifier);

                var calculatedChecksum = (byte)(recordLength + recordType + (byte)recordOffset + (byte)((recordOffset & 0xFF00) >> 8));

                var recordData = new byte[recordLength];
                for (var i = 0; i < recordLength; i++)
                {
                    recordData[i] = byte.Parse(await inputStream.ReadStringAsync(2), NumberStyles.AllowHexSpecifier);
                    calculatedChecksum += recordData[i];
                }

                var recordChecksum = byte.Parse(await inputStream.ReadStringAsync(2), NumberStyles.AllowHexSpecifier);

                // Finalize checksum
                calculatedChecksum = (byte)(~calculatedChecksum + 1);
                if (recordChecksum != calculatedChecksum)
                    throw new InvalidDataException("Invalid checksum value");

                await outputStream.WriteAsync(recordData, 0, recordData.Length);
            }

            return outputStream;
        }
    }
}
