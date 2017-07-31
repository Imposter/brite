using System;
using System.Threading.Tasks;

namespace Brite.Micro
{
    public interface IProgrammer : IDisposable
    {
        Task Open();
        Task Close();

        Task ReadPage(int address, MemoryType type, byte[] data, int offset, int length);
        Task WritePage(int address, MemoryType type, byte[] data, int offset, int length);

        TOperation BeginOperation<TOperation>() where TOperation : ProgrammerOperation, new();
    }
}
