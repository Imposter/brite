using Brite.Micro.Hardware;
using Brite.Utility.IO;
using System.Threading.Tasks;

namespace Brite.Micro.BootloaderProgrammers
{
    public abstract class ArduinoBootloaderProgrammer<TSerialConnection> : SerialBootloaderProgrammer<TSerialConnection> where TSerialConnection : ISerialConnection, new()
    {
        protected int MaxSyncRetries => 20;

        protected abstract Task Reset();

        public ArduinoBootloaderProgrammer(SerialConfig config, IMicrocontroller mcu)
            : base(config, mcu)
        {
        }

        public override async Task Open()
        {
            await base.Open();
            await Reset();
        }

        public override async Task Close()
        {
            await Reset();
            await base.Close();
        }
    }
}
