using System.Threading.Tasks;

namespace Brite.Micro
{
    public abstract class Programmer : IProgrammer
    {
        public abstract Task OpenAsync();
        public abstract Task CloseAsync();
        public abstract Task ReadAsync(MemoryType type, byte[] data, int offset, int length);
        public abstract Task WriteAsync(MemoryType type, byte[] data, int offset, int length);
        public abstract Task EraseAsync();
        public abstract Task ResetAsync();

        public abstract void Dispose();
    }
}
