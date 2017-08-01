using System;
using System.Threading.Tasks;

namespace Brite.Micro
{
    public interface IProgrammer : IDisposable
    {
        Task Open();
        Task Close();

        Task ReadPage(MemoryType type, int address, byte[] data, int offset, int length);
        Task WritePage(MemoryType type, int address, byte[] data, int offset, int length);
    }
}
