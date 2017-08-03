using System;
using System.Threading.Tasks;

namespace Brite.Micro
{
    public interface IProgrammer : IDisposable
    {
        Task OpenAsync();
        Task CloseAsync();

        Task ReadAsync(MemoryType type, byte[] data, int offset, int length);
        Task WriteAsync(MemoryType type, byte[] data, int offset, int length);

        Task EraseAsync();
        Task ResetAsync();
    }
}
