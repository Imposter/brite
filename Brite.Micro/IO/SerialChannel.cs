using System;
using System.Threading.Tasks;
using Brite.Utility.IO;

namespace Brite.Micro.IO
{
    public class SerialChannel : IChannel
    {
        private readonly ISerialConnection _serial;
        private readonly ComPin _resetPin;

        public string Name => _serial.PortName;
        public bool IsOpen => _serial.IsOpen;

        public SerialChannel(ISerialConnection serial, ComPin resetPin)
        {
            _serial = serial;
            _resetPin = resetPin;

            serial.Timeout = 1000;
        }

        public void Dispose()
        {
            _serial.Dispose();
        }

        public async Task Open()
        {
            await _serial.Open();
        }

        public async Task Close()
        {
            await _serial.Close();
        }

        public async Task ToggleReset(bool value)
        {
            await _resetPin.Set(value);
        }

        public async Task SendByte(byte bt)
        {
            await _serial.BaseStream.WriteAsync(new[] { bt }, 0, 1);
        }

        public async Task<byte> ReceiveByte()
        {
            var b = new byte[1];
            if (await _serial.BaseStream.ReadAsync(b, 0, 1) == 0)
                throw new TimeoutException();
            return b[0];
        }
    }
}
