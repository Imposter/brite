/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

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

        private async Task<int> ReadBytesAsync(byte[] buffer, int length)
        {
            if (_peekByte != -1)
            {
                buffer[0] = (byte)_peekByte;
                _peekByte = -1;
                return await _stream.ReadAsync(buffer, 1, length - 1) + 1;
            }

            return await _stream.ReadAsync(buffer, 0, length);
        }

        public async Task<int> PeekAsync()
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

        public async Task<int> ReadAsync()
        {
            var b = new byte[1];
            return await ReadBytesAsync(b, b.Length) < 0 ? -1 : b[0];
        }

        public async Task WriteAsync(byte obj)
        {
            byte[] b = { obj };
            await _stream.WriteAsync(b, 0, b.Length);
        }

        public async Task<bool> ReadAsync(byte[] buffer, int length)
        {
            return await ReadBytesAsync(buffer, length) == length;
        }

        public async Task WriteAsync(byte[] buffer, int length)
        {
            await _stream.WriteAsync(buffer, 0, length);
        }

        public async Task WriteAsync(string str, string encoding = "UTF-8")
        {
            var bytes = Encoding.GetEncoding(encoding).GetBytes(str);
            await _stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private async Task<bool> ReadDataTypeAsync(DataType type)
        {
            if (!_typesEnabled)
                return true;

            var b = await PeekAsync();
            if (b < 0)
                return false;

            if (b >= (int)DataType.Max)
                return false;

            b = await ReadAsync();
            return b == (int)type;
        }

        private async Task WriteDataTypeAsync(DataType type)
        {
            if (_typesEnabled)
                await WriteAsync((byte)type);
        }

        public TypedStream(IStream stream, bool bigEndian = false)
        {
            _stream = stream;
            _bigEndian = bigEndian;
            _peekByte = -1;
        }

        public async Task<bool> ReadBooleanAsync()
        {
            if (!await ReadDataTypeAsync(DataType.Boolean))
                throw new TimeoutException("Unable to read data type");

            var b = await ReadAsync();
            if (b < 0)
                throw new TimeoutException("Unable to read data");

            return b == 1;
        }

        public async Task<sbyte> ReadInt8Async()
        {
            if (!await ReadDataTypeAsync(DataType.Int8))
                throw new TimeoutException("Unable to read data type");

            return (sbyte)await ReadAsync();
        }

        public async Task<byte> ReadUInt8Async()
        {
            if (!await ReadDataTypeAsync(DataType.UInt8))
                throw new TimeoutException("Unable to read data type");

            var b = await ReadAsync();
            if (b < 0)
                throw new TimeoutException("Unable to read data");

            return (byte)b;
        }

        public async Task<short> ReadInt16Async()
        {
            if (!await ReadDataTypeAsync(DataType.Int16))
                throw new TimeoutException("Unable to read data type");

            var buffer = new byte[sizeof(short)];
            if (!await ReadAsync(buffer, buffer.Length))
                throw new TimeoutException("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToInt16(buffer, 0);
        }

        public async Task<ushort> ReadUInt16Async()
        {
            if (!await ReadDataTypeAsync(DataType.UInt16))
                throw new TimeoutException("Unable to read data type");

            var buffer = new byte[sizeof(ushort)];
            if (!await ReadAsync(buffer, buffer.Length))
                throw new TimeoutException("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToUInt16(buffer, 0);
        }

        public async Task<int> ReadInt32Async()
        {
            if (!await ReadDataTypeAsync(DataType.Int32))
                throw new TimeoutException("Unable to read data type");

            var buffer = new byte[sizeof(int)];
            if (!await ReadAsync(buffer, buffer.Length))
                throw new TimeoutException("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToInt32(buffer, 0);
        }

        public async Task<uint> ReadUInt32Async()
        {
            if (!await ReadDataTypeAsync(DataType.UInt32))
                throw new TimeoutException("Unable to read data type");

            var buffer = new byte[sizeof(uint)];
            if (!await ReadAsync(buffer, buffer.Length))
                throw new TimeoutException("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToUInt32(buffer, 0);
        }

        public async Task<float> ReadFloatAsync()
        {
            if (!await ReadDataTypeAsync(DataType.Float))
                throw new TimeoutException("Unable to read data type");

            var buffer = new byte[sizeof(float)];
            if (!await ReadAsync(buffer, buffer.Length))
                throw new TimeoutException("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToSingle(buffer, 0);
        }

        public async Task<string> ReadStringAsync(string encoding = "UTF-8")
        {
            if (!await ReadDataTypeAsync(DataType.String))
                throw new TimeoutException("Unable to read data type");

            var lengthBuffer = new byte[sizeof(int)];
            if (!await ReadAsync(lengthBuffer, lengthBuffer.Length))
                throw new TimeoutException("Unable to read length");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                lengthBuffer.Reverse();

            var length = BitConverter.ToInt32(lengthBuffer, 0);
            var buffer = new byte[length];
            if (!await ReadAsync(buffer, length))
                throw new TimeoutException("Unable to read data");

            return Encoding.GetEncoding(encoding).GetString(buffer);
        }

        public async Task<byte[]> ReadBlobAsync()
        {
            if (!await ReadDataTypeAsync(DataType.Blob))
                throw new TimeoutException("Unable to read data type");

            var lengthBuffer = new byte[sizeof(int)];
            if (!await ReadAsync(lengthBuffer, lengthBuffer.Length))
                throw new TimeoutException("Unable to read length");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                lengthBuffer.Reverse();

            var length = BitConverter.ToInt32(lengthBuffer, 0);
            var obj = new byte[length];
            if (!await ReadAsync(obj, length))
                throw new TimeoutException("Unable to read data");

            return obj;
        }

        public async Task WriteBooleanAsync(bool obj)
        {
            await WriteDataTypeAsync(DataType.Boolean);
            await WriteAsync((byte)(obj ? 1 : 0));
        }

        public async Task WriteInt8Async(sbyte obj)
        {
            await WriteDataTypeAsync(DataType.Int8);
            await WriteAsync((byte)obj);
        }

        public async Task WriteUInt8Async(byte obj)
        {
            await WriteDataTypeAsync(DataType.UInt8);
            await WriteAsync(obj);
        }

        public async Task WriteInt16Async(short obj)
        {
            await WriteDataTypeAsync(DataType.Int16);
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await WriteAsync(buffer, sizeof(short));
        }

        public async Task WriteUInt16Async(ushort obj)
        {
            await WriteDataTypeAsync(DataType.UInt16);
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await WriteAsync(buffer, sizeof(ushort));
        }

        public async Task WriteInt32Async(int obj)
        {
            await WriteDataTypeAsync(DataType.Int32);
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await WriteAsync(buffer, sizeof(int));
        }

        public async Task WriteUInt32Async(uint obj)
        {
            await WriteDataTypeAsync(DataType.UInt32);
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await WriteAsync(buffer, sizeof(uint));
        }

        public async Task WriteFloatAsync(float obj)
        {
            await WriteDataTypeAsync(DataType.Float);
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await WriteAsync(buffer, sizeof(float));
        }

        public async Task WriteStringAsync(string obj, string encoding = "UTF-8")
        {
            // Get encoding
            var encoder = Encoding.GetEncoding(encoding);

            // Calculate length
            var tempBuffer = encoder.GetBytes("A");
            var bytes = encoder.GetBytes(obj);

            await WriteDataTypeAsync(DataType.String);
            var lengthBuffer = BitConverter.GetBytes(bytes.Length * tempBuffer.Length);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                lengthBuffer.Reverse();

            await WriteAsync(lengthBuffer, sizeof(uint));
            await WriteAsync(bytes, bytes.Length);
        }

        public async Task WriteBlobAsync(byte[] obj)
        {
            await WriteDataTypeAsync(DataType.Blob);
            var lengthBuffer = BitConverter.GetBytes(obj.Length);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                lengthBuffer.Reverse();

            await WriteAsync(lengthBuffer, sizeof(uint));
            await WriteAsync(obj, obj.Length);
        }
    }
}
