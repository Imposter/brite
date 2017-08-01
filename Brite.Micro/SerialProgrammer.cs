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

        public override async Task Open()
        {
            await _channel.Open();
        }

        public override async Task Close()
        {
            await _channel.Close();
        }

        public override void Dispose()
        {
        }
    }
}
