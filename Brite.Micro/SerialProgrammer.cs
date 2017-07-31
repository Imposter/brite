using System.Threading.Tasks;

namespace Brite.Micro
{
    public abstract class SerialProgrammer : IProgrammer
    {
        private readonly SerialChannel _channel;

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

        public abstract Task ReadPage(MemoryType type, int address, byte[] data, int offset, int length);
        public abstract Task WritePage(MemoryType type, int address, byte[] data, int offset, int length);

        public virtual void Dispose()
        {
        }
    }
}
