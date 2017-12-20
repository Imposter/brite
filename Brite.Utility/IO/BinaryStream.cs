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
    public class BinaryStream
    {
        private readonly IStream _stream;
        private int _peekByte;
        private bool _bigEndian;

        public IStream Stream => _stream;

        public bool BigEndian
        {
            get => _bigEndian;
            set => _bigEndian = value;
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
            return await ReadBytesAsync(b, b.Length) <= 0 ? -1 : b[0];
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

        public BinaryStream(IStream stream, bool bigEndian = false)
        {
            _stream = stream;
            _bigEndian = bigEndian;
            _peekByte = -1;
        }

        public async Task<bool> ReadBooleanAsync()
        {
            var b = await ReadAsync();
            if (b < 0)
                return false;

            return b == 1;
        }

        public async Task<char> ReadCharAsync()
        {
            var b = await ReadAsync();
            if (b < 0)
                throw new TimeoutException("Unable to read data");

            return (char)b;
        }

        public async Task<sbyte> ReadInt8Async()
        {
            return (sbyte)await ReadAsync();
        }

        public async Task<byte> ReadUInt8Async()
        {
            var b = await ReadAsync();
            if (b < 0)
                throw new TimeoutException("Unable to read data");

            return (byte)b;
        }

        public async Task<short> ReadInt16Async()
        {
            var buffer = new byte[sizeof(short)];
            if (!await ReadAsync(buffer, buffer.Length))
                throw new TimeoutException("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToInt16(buffer, 0);
        }

        public async Task<ushort> ReadUInt16Async()
        {
            var buffer = new byte[sizeof(ushort)];
            if (!await ReadAsync(buffer, buffer.Length))
                throw new TimeoutException("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToUInt16(buffer, 0);
        }

        public async Task<int> ReadInt32Async()
        {
            var buffer = new byte[sizeof(int)];
            if (!await ReadAsync(buffer, buffer.Length))
                throw new TimeoutException("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToInt32(buffer, 0);
        }

        public async Task<uint> ReadUInt32Async()
        {
            var buffer = new byte[sizeof(uint)];
            if (!await ReadAsync(buffer, buffer.Length))
                throw new TimeoutException("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToUInt32(buffer, 0);
        }

        public async Task<float> ReadFloatAsync()
        {
            var buffer = new byte[sizeof(float)];
            if (!await ReadAsync(buffer, buffer.Length))
                throw new TimeoutException("Unable to read data");

            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToSingle(buffer, 0);
        }

        public async Task<string> ReadStringAsync(int length, string encoding = "UTF-8")
        {
            // Get encoding
            var encoder = Encoding.GetEncoding(encoding);

            // Calculate length
            var tempBuffer = encoder.GetBytes("A");
            length *= tempBuffer.Length;

            var buffer = new byte[length];
            if (!await ReadAsync(buffer, length))
                throw new TimeoutException("Unable to read data");

            return Encoding.GetEncoding(encoding).GetString(buffer);
        }

        public async Task<byte[]> ReadBlobAsync(int length)
        {
            var obj = new byte[length];
            if (!await ReadAsync(obj, length))
                throw new TimeoutException("Unable to read data");

            return obj;
        }

        public async Task WriteBooleanAsync(bool obj)
        {
            await WriteAsync((byte)(obj ? 1 : 0));
        }

        public async Task WriteCharAsync(char obj)
        {
            await WriteAsync((byte)obj);
        }

        public async Task WriteInt8Async(sbyte obj)
        {
            await WriteAsync((byte)obj);
        }

        public async Task WriteUInt8Async(byte obj)
        {
            await WriteAsync(obj);
        }

        public async Task WriteInt16Async(short obj)
        {
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await WriteAsync(buffer, sizeof(short));
        }

        public async Task WriteUInt16Async(ushort obj)
        {
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await WriteAsync(buffer, sizeof(ushort));
        }

        public async Task WriteInt32Async(int obj)
        {
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await WriteAsync(buffer, sizeof(int));
        }

        public async Task WriteUInt32Async(uint obj)
        {
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await WriteAsync(buffer, sizeof(uint));
        }

        public async Task WriteFloatAsync(float obj)
        {
            var buffer = BitConverter.GetBytes(obj);
            if (_bigEndian && BitConverter.IsLittleEndian || !_bigEndian && !BitConverter.IsLittleEndian)
                buffer.Reverse();

            await WriteAsync(buffer, sizeof(float));
        }

        public async Task WriteStringAsync(string obj, string encoding = "UTF-8")
        {
            await WriteAsync(obj, encoding);
        }

        public async Task WriteBlobAsync(byte[] obj)
        {
            await WriteAsync(obj, obj.Length);
        }
    }
}
