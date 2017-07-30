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

        public IStream Stream => _stream;

        public bool TypesEnabled
        {
            get => _typesEnabled;
            set => _typesEnabled = value;
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

        private async Task<int> StreamRead(byte[] buffer, int offset, int length)
        {
            // Don't attempt to read unnecessarily
            if (length <= 0)
                return 0;

            return await _stream.ReadAsync(buffer, offset, length);
        }

        private async Task<int> ReadBytes(byte[] buffer, int length)
        {
            if (_peekByte != -1)
            {
                buffer[0] = (byte)_peekByte;
                _peekByte = -1;
                return await StreamRead(buffer, 1, length - 1) + 1;
            }

            return await StreamRead(buffer, 0, length);
        }

        public async Task<int> Read()
        {
            var b = new byte[1];
            return await ReadBytes(b, b.Length) <= 0 ? -1 : b[0];
        }

        public async void Write(byte obj)
        {
            byte[] b = { obj };
            await _stream.WriteAsync(b, 0, b.Length);
        }

        public async Task<bool> Read(byte[] buffer, int length)
        {
            return await ReadBytes(buffer, length) == length;
        }

        public async void Write(byte[] buffer, int length)
        {
            await _stream.WriteAsync(buffer, 0, length);
        }

        public async void Write(string str)
        {
            await _stream.WriteAsync(Encoding.ASCII.GetBytes(str), 0, str.Length);
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

        private void WriteDataType(DataType type)
        {
            if (_typesEnabled)
                Write((byte)type);
        }

        public TypedStream(IStream stream)
        {
            _stream = stream;
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

            return BitConverter.ToInt16(buffer, 0);
        }

        public async Task<ushort> ReadUInt16()
        {
            if (!await ReadDataType(DataType.UInt16))
                throw new Exception("Unable to read data type");

            var buffer = new byte[sizeof(ushort)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            return BitConverter.ToUInt16(buffer, 0);
        }

        public async Task<int> ReadInt32()
        {
            if (!await ReadDataType(DataType.Int32))
                throw new Exception("Unable to read data type");

            var buffer = new byte[sizeof(int)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            return BitConverter.ToInt32(buffer, 0);
        }

        public async Task<uint> ReadUInt32()
        {
            if (!await ReadDataType(DataType.UInt32))
                throw new Exception("Unable to read data type");

            var buffer = new byte[sizeof(uint)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            return BitConverter.ToUInt32(buffer, 0);
        }

        public async Task<float> ReadFloat()
        {
            if (!await ReadDataType(DataType.Float))
                throw new Exception("Unable to read data type");

            var buffer = new byte[sizeof(float)];
            if (!await Read(buffer, buffer.Length))
                throw new Exception("Unable to read data");

            return BitConverter.ToSingle(buffer, 0);
        }

        public async Task<string> ReadString()
        {
            if (!await ReadDataType(DataType.String))
                throw new Exception("Unable to read data type");

            var lengthBuffer = new byte[sizeof(int)];
            if (!await Read(lengthBuffer, lengthBuffer.Length))
                throw new Exception("Unable to read length");

            var length = BitConverter.ToInt32(lengthBuffer, 0);
            var buffer = new byte[length];
            if (!await Read(buffer, length))
                throw new Exception("Unable to read data");

            return Encoding.ASCII.GetString(buffer);
        }

        public async Task<byte[]> ReadBlob()
        {
            if (!await ReadDataType(DataType.Blob))
                throw new Exception("Unable to read data type");

            var lengthBuffer = new byte[sizeof(int)];
            if (!await Read(lengthBuffer, lengthBuffer.Length))
                throw new Exception("Unable to read length");

            var length = BitConverter.ToInt32(lengthBuffer, 0);
            var obj = new byte[length];
            if (!await Read(obj, length))
                throw new Exception("Unable to read data");

            return obj;
        }

        public void WriteBoolean(bool obj)
        {
            WriteDataType(DataType.Boolean);
            Write((byte)(obj ? 1 : 0));
        }

        public void WriteInt8(sbyte obj)
        {
            WriteDataType(DataType.Int8);
            Write((byte)obj);
        }

        public void WriteUInt8(byte obj)
        {
            WriteDataType(DataType.UInt8);
            Write(obj);
        }

        public void WriteInt16(short obj)
        {
            WriteDataType(DataType.Int16);
            Write(BitConverter.GetBytes(obj), sizeof(short));
        }

        public void WriteUInt16(ushort obj)
        {
            WriteDataType(DataType.UInt16);
            Write(BitConverter.GetBytes(obj), sizeof(ushort));
        }

        public void WriteInt32(int obj)
        {
            WriteDataType(DataType.Int32);
            Write(BitConverter.GetBytes(obj), sizeof(int));
        }

        public void WriteUInt32(uint obj)
        {
            WriteDataType(DataType.UInt32);
            Write(BitConverter.GetBytes(obj), sizeof(uint));
        }

        public void WriteFloat(float obj)
        {
            WriteDataType(DataType.Float);
            Write(BitConverter.GetBytes(obj), sizeof(float));
        }

        public void WriteString(string obj)
        {
            WriteDataType(DataType.String);
            Write(BitConverter.GetBytes(obj.Length), sizeof(uint));
            Write(Encoding.ASCII.GetBytes(obj), obj.Length);
        }

        public void WriteBlob(byte[] obj)
        {
            WriteDataType(DataType.Blob);
            Write(BitConverter.GetBytes(obj.Length), sizeof(uint));
            Write(obj, obj.Length);
        }
    }
}
