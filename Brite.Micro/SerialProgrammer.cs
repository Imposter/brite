using Brite.Utility.IO;
using System.Threading.Tasks;

namespace Brite.Micro
{
    public abstract class SerialProgrammer : IProgrammer
    {
        private SerialChannel _channel;

        public SerialChannel Channel => _channel;

        public SerialProgrammer(SerialChannel channel)
        {
            _channel = channel;
        }

        public virtual async Task Open()
        {
            await _channel.Open();
        }

        public virtual async Task Close()
        {
            await _channel.Close();
        }

        public T BeginOperation<T>() where T : ProgrammerOperation, new()
        {
            var operation = new T();
            operation.Initialize(this);
            return operation;
        }

        public abstract Task ReadPage(int address, MemoryType type, byte[] data, int offset, int length);
        public abstract Task WritePage(int address, MemoryType type, byte[] data, int offset, int length);

        public virtual void Dispose()
        {
        }
    }
}
