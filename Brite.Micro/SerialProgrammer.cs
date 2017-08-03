using System.Threading.Tasks;

namespace Brite.Micro
{
    public abstract class SerialProgrammer : Programmer
    {
        private readonly SerialChannel _channel;

        public SerialChannel Channel => _channel;

        public SerialProgrammer(SerialChannel channel)
        {
            _channel = channel;
        }

        public override async Task OpenAsync()
        {
            await _channel.OpenAsync();
        }

        public override async Task CloseAsync()
        {
            await _channel.CloseAsync();
        }

        public override void Dispose()
        {
        }
    }
}
