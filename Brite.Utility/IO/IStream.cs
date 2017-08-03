using System;
using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public interface IStream : IDisposable
    {
        Task<int> ReadAsync(byte[] buffer, int offset, int length);
        Task WriteAsync(byte[] buffer, int offset, int length);
    }
}