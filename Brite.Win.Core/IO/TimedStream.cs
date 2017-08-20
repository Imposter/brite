using System;
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
            var task = _stream.ReadAsync(buffer, offset, length);
            if (await Task.WhenAny(task, Task.Delay(Timeout)) == task)
                return task.Result;

            throw new TimeoutException("Failed to read within specified time");
        }

        public async Task WriteAsync(byte[] buffer, int offset, int length)
        {
            var task = _stream.WriteAsync(buffer, offset, length);
            if (await Task.WhenAny(task, Task.Delay(Timeout)) == task)
                return;

            throw new TimeoutException("Failed to write within specified time");
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
