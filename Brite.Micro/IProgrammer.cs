using System;
using System.Threading.Tasks;

namespace Brite.Micro
{
    public interface IProgrammer : IDisposable
    {
        Task<ProgrammingSession> Start();
        Task Stop();

        Task ReadPage(int address, MemoryType memType, byte[] data, int dataStart, int dataLength);
        Task WritePage(int address, MemoryType memType, byte[] data, int dataStart, int dataLength);

        Task EraseDevice();
    }
}
