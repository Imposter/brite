using System;
using System.Text;
using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public class TypedStream
    {
        private enum DataType : byte
        {
            Boolean,
            Int8,
            UInt8,
            Int16,
            UInt16,
            Int32,
            UInt32,
            Float,
            String,
            Blob,
            Max
        }

        private readonly IStream _stream;
        private bool _typesEnabled;
        private int _peekByte;
        private bool _bigEndian;

        public IStream Stream => _stream;

        public bool BigEndian
        {
            get => _bigEndian;
            set => _bigEndian = value;
        }

        public bool TypesEnabled
        {
            get => _typesEnabled;
            set => _typesEnabled = value;
        }

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

        private async Task<bool> ReadDataType(DataType type)
        {
            if (!_typesEnabled)
                return true;

            var b = await Peek();
            if (b < 0)
                return false;

            if (b >= (int)DataType.Max)
                return false;

            b = await Read();
            return b == (int)type;
        }

        private async Task WriteDataType(DataType type)
        {
            if (_typesEnabled)
                await Write((byte)type);
        }

        public TypedStream(IStream stream, bool bigEndian = false)
        {
            _stream = stream;
            _bigEndian = bigEndian;
            _peekByte = -1;
        }

        public async Task<bool> ReadBoolean()
        {
            if (!await ReadDataType(DataType.Boolean))
                throw new Exception("Unable to read data type");

            var b = await Read();
            if (b < 0)
                return false;

            return b == 1;
        }

        public async Task<sbyte> ReadInt8()
        {
            if (!await ReadDataType(DataType.Int8))
                throw new Exception("Unable to read data type");

            return (sbyte)await Read();
        }

        public async Task<byte> ReadUInt8()
        {
            if (!await ReadDataType(DataType.UInt8))
                throw new Exception("Unable to read data type");

            var b = await Read();
            if (b < 0)
                throw new Exception("Unable to read data");

            return (byte)b;
        }

        public async Task<short> ReadInt16()
        {
            if (!await ReadDataType(DataType.Int16))
                throw new Exception("Unable to read data type");

            var buffer = new byte[sizeof(short)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToInt16(buffer, 0);
        }

        public async Task<ushort> ReadUInt16()
        {
            if (!await ReadDataType(DataType.UInt16))
                throw new Exception("Unable to read data type");

            var buffer = new byte[sizeof(ushort)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToUInt16(buffer, 0);
        }

        public async Task<int> ReadInt32()
        {
            if (!await ReadDataType(DataType.Int32))
                throw new Exception("Unable to read data type");

            var buffer = new byte[sizeof(int)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToInt32(buffer, 0);
        }

        public async Task<uint> ReadUInt32()
        {
            if (!await ReadDataType(DataType.UInt32))
                throw new Exception("Unable to read data type");

            var buffer = new byte[sizeof(uint)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToUInt32(buffer, 0);
        }

        public async Task<float> ReadFloat()
        {
            if (!await ReadDataType(DataType.Float))
                throw new Exception("Unable to read data type");

            var buffer = new byte[sizeof(float)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToSingle(buffer, 0);
        }

        public async Task<string> ReadString(string encoding = "UTF-8")
        {
            if (!await ReadDataType(DataType.String))
                throw new Exception("Unable to read data type");

            var lengthBuffer = new byte[sizeof(int)];
            if (!await Read(lengthBuffer, lengthBuffer.Length))
                throw new Exception("Unable to read length");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                lengthBuffer.Reverse();

            var length = BitConverter.ToInt32(lengthBuffer, 0);
            var buffer = new byte[length];
            if (!await Read(buffer, length))
                throw new Exception("Unable to read data");

            return Encoding.GetEncoding(encoding).GetString(buffer);
        }

        public async Task<byte[]> ReadBlob()
        {
            if (!await ReadDataType(DataType.Blob))
                throw new Exception("Unable to read data type");

            var lengthBuffer = new byte[sizeof(int)];
            if (!await Read(lengthBuffer, lengthBuffer.Length))
                throw new Exception("Unable to read length");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                lengthBuffer.Reverse();

            var length = BitConverter.ToInt32(lengthBuffer, 0);
            var obj = new byte[length];
            if (!await Read(obj, length))
                throw new Exception("Unable to read data");

            return obj;
        }

        public async Task WriteBoolean(bool obj)
        {
            await WriteDataType(DataType.Boolean);
            await Write((byte)(obj ? 1 : 0));
        }

        public async Task WriteInt8(sbyte obj)
        {
            await WriteDataType(DataType.Int8);
            await Write((byte)obj);
        }

        public async Task WriteUInt8(byte obj)
        {
            await WriteDataType(DataType.UInt8);
            await Write(obj);
        }

        public async Task WriteInt16(short obj)
        {
            await WriteDataType(DataType.Int16);
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await Write(buffer, sizeof(short));
        }

        public async Task WriteUInt16(ushort obj)
        {
            await WriteDataType(DataType.UInt16);
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await Write(buffer, sizeof(ushort));
        }

        public async Task WriteInt32(int obj)
        {
            await WriteDataType(DataType.Int32);
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await Write(buffer, sizeof(int));
        }

        public async Task WriteUInt32(uint obj)
        {
            await WriteDataType(DataType.UInt32);
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await Write(buffer, sizeof(uint));
        }

        public async Task WriteFloat(float obj)
        {
            await WriteDataType(DataType.Float);
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await Write(buffer, sizeof(float));
        }

        public async Task WriteString(string obj, string encoding = "UTF-8")
        {
            // Get encoding
            var encoder = Encoding.GetEncoding(encoding);

            // Calculate length
            var tempBuffer = encoder.GetBytes("A");
            var bytes = encoder.GetBytes(obj);

            await WriteDataType(DataType.String);
            var lengthBuffer = BitConverter.GetBytes(bytes.Length * tempBuffer.Length);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                lengthBuffer.Reverse();

            await Write(lengthBuffer, sizeof(uint));
            await Write(bytes, bytes.Length);
        }

        public async Task WriteBlob(byte[] obj)
        {
            await WriteDataType(DataType.Blob);
            var lengthBuffer = BitConverter.GetBytes(obj.Length);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                lengthBuffer.Reverse();

            await Write(lengthBuffer, sizeof(uint));
            await Write(obj, obj.Length);
        }
    }
}
