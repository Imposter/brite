using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public class BinaryStream
    {
        private readonly IStream _stream;
        private int _peekByte;

        public IStream Stream => _stream;

        private async Task<int> ReadBytes(byte[] buffer, int length)
        {
            if (_peekByte != -1)
            {
                buffer[0] = (byte)_peekByte;
                _peekByte = -1;
                return await _stream.ReadAsync(buffer, 1, length - 1) + 1;
            }

            return await _stream.ReadAsync(buffer, 0, length);
        }

        public async Task<int> Peek()
        {
            if (_peekByte != -1)
                return _peekByte;

            var b = new byte[1];
            if (await _stream.ReadAsync(b, 0, b.Length) > 0)
            {
                _peekByte = b[0];
                return _peekByte;
            }

            return -1;
        }

        public async Task<int> Read()
        {
            var b = new byte[1];
            return await ReadBytes(b, b.Length) <= 0 ? -1 : b[0];
        }

        public async Task Write(byte obj)
        {
            byte[] b = { obj };
            await _stream.WriteAsync(b, 0, b.Length);
        }

        public async Task<bool> Read(byte[] buffer, int length)
        {
            return await ReadBytes(buffer, length) == length;
        }

        public async Task Write(byte[] buffer, int length)
        {
            await _stream.WriteAsync(buffer, 0, length);
        }

        public async Task Write(string str, string encoding = "UTF-8")
        {
            var bytes = Encoding.GetEncoding(encoding).GetBytes(str);
            await _stream.WriteAsync(bytes, 0, bytes.Length);
        }

        public BinaryStream(IStream stream)
        {
            _stream = stream;
            _peekByte = -1;
        }

        public async Task<bool> ReadBoolean()
        {
            var b = await Read();
            if (b < 0)
                return false;

            return b == 1;
        }

        public async Task<sbyte> ReadInt8()
        {
            return (sbyte)await Read();
        }

        public async Task<byte> ReadUInt8()
        {
            var b = await Read();
            if (b < 0)
                throw new Exception("Unable to read data");

            return (byte)b;
        }

        public async Task<short> ReadInt16()
        {
            var buffer = new byte[sizeof(short)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            return BitConverter.ToInt16(buffer, 0);
        }

        public async Task<ushort> ReadUInt16()
        {
            var buffer = new byte[sizeof(ushort)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            return BitConverter.ToUInt16(buffer, 0);
        }

        public async Task<int> ReadInt32()
        {
            var buffer = new byte[sizeof(int)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            return BitConverter.ToInt32(buffer, 0);
        }

        public async Task<uint> ReadUInt32()
        {
            var buffer = new byte[sizeof(uint)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            return BitConverter.ToUInt32(buffer, 0);
        }

        public async Task<float> ReadFloat()
        {
            var buffer = new byte[sizeof(float)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            return BitConverter.ToSingle(buffer, 0);
        }

        public async Task<string> ReadString(int length, string encoding = "UTF-8")
        {
            // Get encoding
            var encoder = Encoding.GetEncoding(encoding);

            // Calculate length
            var tempBuffer = encoder.GetBytes("A");
            length *= tempBuffer.Length;

            var buffer = new byte[length];
            if (!await Read(buffer, length))
                throw new Exception("Unable to read data");

            return Encoding.GetEncoding(encoding).GetString(buffer);
        }

        public async Task<byte[]> ReadBlob(int length)
        {
           var obj = new byte[length];
            if (!await Read(obj, length))
                throw new Exception("Unable to read data");

            return obj;
        }

        public async Task WriteBoolean(bool obj)
        {
            await Write((byte)(obj ? 1 : 0));
        }

        public async Task WriteInt8(sbyte obj)
        {
            await Write((byte)obj);
        }

        public async Task WriteUInt8(byte obj)
        {
            await Write(obj);
        }

        public async Task WriteInt16(short obj)
        {
            await Write(BitConverter.GetBytes(obj), sizeof(short));
        }

        public async Task WriteUInt16(ushort obj)
        {
            await Write(BitConverter.GetBytes(obj), sizeof(ushort));
        }

        public async Task WriteInt32(int obj)
        {
            await Write(BitConverter.GetBytes(obj), sizeof(int));
        }

        public async Task WriteUInt32(uint obj)
        {
            await Write(BitConverter.GetBytes(obj), sizeof(uint));
        }

        public async Task WriteFloat(float obj)
        {
            await Write(BitConverter.GetBytes(obj), sizeof(float));
        }

        public async Task WriteString(string obj, string encoding = "UTF-8")
        {
            await Write(obj, encoding);
        }

        public async Task WriteBlob(byte[] obj)
        {
            await Write(obj, obj.Length);
        }
    }
}
