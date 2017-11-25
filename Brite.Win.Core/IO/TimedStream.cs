using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Brite.Utility.IO;
using IOStream = System.IO.Stream;

namespace Brite.Win.Core.IO
{
    public class TimedStream : ITimedStream
    {
        public const int DefaultTimeout = 100; // ms

        private readonly IOStream _stream;

        public int Timeout { get; set; }

        public TimedStream(IOStream underlyingStream, int timeout = DefaultTimeout)
        {
            _stream = underlyingStream;
            Timeout = timeout;
        }

        public async Task<int> ReadAsync(byte[] buffer, int offset, int length)
        {
            if (length <= 0)
                return 0;

            var task = ReadInternalAsync(buffer, offset, length);
            if (await Task.WhenAny(task, Task.Delay(Timeout)) == task)
                return task.Result;

            throw new TimeoutException("Failed to read within specified time");
        }

        public async Task WriteAsync(byte[] buffer, int offset, int length)
        {
            if (length <= 0)
                return;

            var task = _stream.WriteAsync(buffer, offset, length);
            if (await Task.WhenAny(task, Task.Delay(Timeout)) == task)
                return;

            throw new TimeoutException("Failed to write within specified time");
        }
        
        private async Task<int> ReadInternalAsync(byte[] buffer, int offset, int length)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var readBytes = 0;
            while (Timeout < 0 && readBytes < length || stopwatch.ElapsedMilliseconds < Timeout)
            {
                var task = _stream.ReadAsync(buffer, offset + readBytes, length - readBytes);
                if (await Task.WhenAny(task, Task.Delay(Timeout)) == task)
                    readBytes += task.Result;
            }

            stopwatch.Stop();

            return readBytes;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
