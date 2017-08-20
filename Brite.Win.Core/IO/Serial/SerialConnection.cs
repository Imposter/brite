using Brite.Utility.IO;
using Brite.Utility.IO.Serial;
using System;
using System.IO.Ports;
using System.Threading.Tasks;
using SerialParity = Brite.Utility.IO.Serial.SerialParity;
using WinSerialParity = System.IO.Ports.Parity;
using WinSerialStopBits = System.IO.Ports.StopBits;
using WinSerialHandshake= System.IO.Ports.Handshake;

namespace Brite.Win.Core.IO.Serial
{
    public class SerialConnection : ISerialConnection
    {
        public const int DefaultTimeout = 100; // ms

        private SerialPort _port;
        private TimedStream _stream;

        private bool _dtrEnable;
        private bool _rtsEnable;
        private int _timeout;

        public bool IsOpen => _port != null;
        public bool DtrEnable
        {
            get => _dtrEnable;
            set
            {
                _dtrEnable = value;
                if (_port != null)
                    _port.DtrEnable = value;
            }
        }
        public bool RtsEnable
        {
            get => _rtsEnable;
            set
            {
                _rtsEnable = value;
                if (_port != null)
                    _port.RtsEnable = value;
            }
        }
        public string PortName { get; set; }
        public uint BaudRate { get; set; }
        public int Timeout
        {
            get => _timeout;
            set
            {
                _timeout = value;
                if (_stream != null)
                    _stream.Timeout = value;
            }
        }
        public bool CtsHolding => _port.CtsHolding;
        public bool CdHolding => _port.CDHolding;
        public bool DsrHolding => _port.DsrHolding;

        // Require connection to be reset
        public ushort DataBits { get; set; }
        public SerialStopBits StopBits { get; set; }
        public SerialParity Parity { get; set; }
        public SerialHandshake Handshake { get; set; }

        public IStream BaseStream => _stream;

        public SerialConnection()
        {
            BaudRate = 4800;
            Timeout = DefaultTimeout;
            DtrEnable = false;
            RtsEnable = false;
            DataBits = 8;
            StopBits = SerialStopBits.One;
            Parity = SerialParity.None;
        }

        public SerialConnection(string portName, uint baudRate, int timeout = DefaultTimeout)
        {
            PortName = portName;
            BaudRate = baudRate;
            Timeout = timeout;

            DtrEnable = false;
            RtsEnable = false;
            DataBits = 8;
            StopBits = SerialStopBits.One;
            Parity = SerialParity.None;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task OpenAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_port != null)
                throw new InvalidOperationException("Port is already open");

            _port = new SerialPort();
            _port.PortName = PortName;
            _port.BaudRate = (int)BaudRate;
            _port.DataBits = DataBits;
            _port.DtrEnable = DtrEnable;
            _port.RtsEnable = RtsEnable;

            switch (StopBits)
            {
                case SerialStopBits.One:
                    _port.StopBits = WinSerialStopBits.One;
                    break;
                case SerialStopBits.OnePointFive:
                    _port.StopBits = WinSerialStopBits.OnePointFive;
                    break;
                case SerialStopBits.Two:
                    _port.StopBits = WinSerialStopBits.Two;
                    break;
                case SerialStopBits.None:
                    _port.StopBits = WinSerialStopBits.None;
                    break;
            }

            switch (Parity)
            {
                case SerialParity.None:
                    _port.Parity = WinSerialParity.None;
                    break;
                case SerialParity.Odd:
                    _port.Parity = WinSerialParity.Odd;
                    break;
                case SerialParity.Even:
                    _port.Parity = WinSerialParity.Even;
                    break;
                case SerialParity.Mark:
                    _port.Parity = WinSerialParity.Mark;
                    break;
                case SerialParity.Space:
                    _port.Parity = WinSerialParity.Space;
                    break;
            }

            switch (Handshake)
            {
                case SerialHandshake.None:
                    _port.Handshake = WinSerialHandshake.None;
                    break;
                case SerialHandshake.RequestToSend:
                    _port.Handshake = WinSerialHandshake.RequestToSend;
                    break;
                case SerialHandshake.XOnXOff:
                    _port.Handshake = WinSerialHandshake.XOnXOff;
                    break;
                case SerialHandshake.RequestToSendXOnXOff:
                    _port.Handshake = WinSerialHandshake.RequestToSend;
                    break;
            }

            _port.Open();

            _stream = new TimedStream(_port.BaseStream, Timeout);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task CloseAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_port == null)
                throw new InvalidOperationException("Port is not open");

            _port.Dispose();
            _stream.Dispose();

            _port = null;
            _stream = null;

        }

        public void Dispose()
        {
            if (_port != null)
            {
                _port.Dispose();
                _stream.Dispose();

                _port = null;
                _stream = null;
            }
        }
    }
}
