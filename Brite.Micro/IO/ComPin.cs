using System;
using System.Threading.Tasks;
using Brite.Utility.IO;

namespace Brite.Micro.IO
{
    public class ComPin
    {
        private readonly ISerialConnection _serial;
        private readonly ComPinType _pin;
        private readonly bool _invert;

        public ComPin(ISerialConnection serial, ComPinType pin, bool invert)
        {
            _serial = serial;
            _pin = pin;
            _invert = invert;
        }

        public async void Reset()
        {
            await Set(false);
        }

        public bool Get()
        {
            switch (_pin)
            {
                case ComPinType.Cts:
                    return _serial.CtsHolding ^ _invert;
                case ComPinType.CD:
                    return _serial.CdHolding ^ _invert;
                case ComPinType.Dsr:
                    return _serial.DsrHolding ^ _invert;
                case ComPinType.None:
                    return _invert;
                default:
                    throw new NotSupportedException();
            }
        }

        public async Task Set(bool value = true)
        {
            switch (_pin)
            {
                case ComPinType.Rts:
                    _serial.RtsEnable = value ^ _invert;
                    break;
                case ComPinType.Dtr:
                    _serial.DtrEnable = value ^ _invert;
                    break;
                case ComPinType.TxD:
                    var b = new[] { (byte)((value ^ _invert) ? 0xff : 0x00) };
                    await _serial.BaseStream.WriteAsync(b, 0, 1);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
