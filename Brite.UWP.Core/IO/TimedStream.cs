using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Brite.Utility.IO;

namespace Brite.UWP.Core.IO
{
    public class TimedStream : ITimedStream
    {
        public const int DefaultTimeout = 100; // ms

        private readonly DataReader _reader;
        private readonly DataWriter _writer;

        public int Timeout { get; set; }

        public TimedStream(IInputStream input, int timeout = DefaultTimeout)
        {
            _reader = new DataReader(input);
            Timeout = timeout;
        }

        public TimedStream(IOutputStream output, int timeout = DefaultTimeout)
        {
            _writer = new DataWriter(output);
            Timeout = timeout;
        }

        public TimedStream(IInputStream input, IOutputStream output, int timeout = DefaultTimeout)
        {
            _reader = new DataReader(input);
            _writer = new DataWriter(output);
            Timeout = timeout;
        }

        public async Task<int> ReadAsync(byte[] buffer, int offset, int length)
        {
            if (_reader == null)
                throw new InvalidOperationException("Reading not supported by underlying stream");

            if (length <= 0)
                return 0;

            var task = ReadInternalAsync(buffer, offset, length);
            if (await Task.WhenAny(task, Task.Delay(Timeout)) == task)
                return task.Result;

            throw new TimeoutException("Failed to read within specified time");
        }

        public async Task WriteAsync(byte[] buffer, int offset, int length)
        {
            if (_writer == null)
                throw new InvalidOperationException("Writing not supported by underlying stream");

            if (length <= 0)
                return;

            var task = WriteInternalAsync(buffer, offset);
            if (await Task.WhenAny(task, Task.Delay(Timeout)) == task)
                return;

            throw new TimeoutException("Failed to write within specified time");
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
        }

        private async Task<int> ReadInternalAsync(byte[] buffer, int offset, int length)
        {
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

        private async Task WriteInternalAsync(byte[] buffer, int offset)
        {
            _writer.WriteBytes(offset > 0 ? buffer.Skip(offset).ToArray() : buffer);
            await _writer.StoreAsync();
        }
    }
}
