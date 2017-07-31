using Brite.Utility.IO;
using System;
using System.Threading.Tasks;

namespace Brite.Micro
{
    public class SerialChannel : IChannel
    {
        public const int DefaultTimeout = 2000;

        private readonly ISerialConnection _serial;
        private readonly SerialPinType _pin;
        private readonly bool _invert;
        private BinaryStream _stream;

        public bool IsOpen => _serial.IsOpen;
        public BinaryStream Stream => _stream;
        public int Timeout
        {
            get => _serial.Timeout;
            set => _serial.Timeout = value;
        }

        public SerialChannel(ISerialConnection serial, SerialPinType pin, bool invert = false)
        {
            _serial = serial;
            _pin = pin;
            _invert = invert;

            _serial.Timeout = DefaultTimeout;
        }

        public async Task Open()
        {
            await _serial.Open();
            _stream = new BinaryStream(_serial.BaseStream);
        }

        public async Task Close()
        {
            await _serial.Close();
        }

        public async Task ToggleReset(bool reset)
        {
            switch (_pin)
            {
                case SerialPinType.Rts:
                    _serial.RtsEnable = reset ^ _invert;
                    break;
                case SerialPinType.Dtr:
                    _serial.DtrEnable = reset ^ _invert;
                    break;
                case SerialPinType.TxD:
                    await _stream.WriteUInt8((byte)(reset ^ _invert ? 0xff : 0x00));
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void Dispose()
        {
        }
    }
}
