using System;
using System.Threading.Tasks;

namespace Brite.Micro
{
    public abstract class Programmer : IProgrammer
    {
        public abstract Task Open();
        public abstract Task Close();
        public abstract Task ReadPage(MemoryType type, int address, byte[] data, int offset, int length);
        public abstract Task WritePage(MemoryType type, int address, byte[] data, int offset, int length);
        public abstract void Dispose();
    }
}
