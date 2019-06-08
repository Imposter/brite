/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Linq;
using System.Threading.Tasks;

using Brite.Utility.IO;

using Windows.Storage.Streams;

namespace Brite.UWP.Core.IO
{
    public class Stream : IStream
    {
        private readonly DataReader _reader;
        private readonly DataWriter _writer;

        public Stream(IInputStream input)
        {
            _reader = new DataReader(input);
        }

        public Stream(IOutputStream output)
        {
            _writer = new DataWriter(output);
        }

        public Stream(IInputStream input, IOutputStream output)
        {
            _reader = new DataReader(input);
            _writer = new DataWriter(output);
        }

        public async Task<int> ReadAsync(byte[] buffer, int offset, int length)
        {
            if (_reader == null) 
                throw new InvalidOperationException("Reading not supported by underlying stream");

            if (length <= 0)
                return 0;

            var readCount = await _reader.LoadAsync((uint)length);
            if (offset > 0 || readCount < length)
            {
                var tempBuffer = new byte[readCount];
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
            if (_writer == null)
                throw new InvalidOperationException("Writing not supported by underlying stream");

            if (length <= 0)
                return;

            _writer.WriteBytes(offset > 0 ? buffer.Skip(offset).ToArray() : buffer);
            await _writer.StoreAsync();
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
        }
    }
}
