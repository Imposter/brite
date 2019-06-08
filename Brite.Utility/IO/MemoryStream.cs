/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

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
