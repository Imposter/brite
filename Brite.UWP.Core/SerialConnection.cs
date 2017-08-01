using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Brite.Utility.IO;
using SerialParity = Brite.Utility.IO.SerialParity;
using WinSerialStopBits = Windows.Devices.SerialCommunication.SerialStopBitCount;
using WinSerialParity = Windows.Devices.SerialCommunication.SerialParity;

namespace Brite.UWP.Core
{
    public class SerialConnection : ISerialConnection
    {
        public const int DefaultTimeout = 100; // ms

        private SerialDevice _device;
        private DeviceInformation _deviceInformation;
        private Stream _stream;

        private bool _dtrEnable;
        private bool _rtsEnable;

        public string PortName { get; set; }
        public uint BaudRate { get; set; }
        public int Timeout { get; set; }

        public bool IsOpen => _device != null;
        public bool DtrEnable
        {
            get => _dtrEnable;
            set
            {
                _dtrEnable = value;
                if (_device != null)
                    _device.IsDataTerminalReadyEnabled = value;
            }
        }
        public bool RtsEnable
        {
            get => _rtsEnable;
            set
            {
                _rtsEnable = value;
                if (_device != null)
                    _device.IsRequestToSendEnabled = value;
            }
        }

        public bool CtsHolding => _device.ClearToSendState;
        public bool CdHolding => _device.CarrierDetectState;
        public bool DsrHolding => _device.DataSetReadyState;

        public ushort DataBits { get; set; }
        public SerialStopBits StopBits { get; set; }
        public SerialParity Parity { get; set; }
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

        public async Task Open()
        {
            var selector = SerialDevice.GetDeviceSelector(PortName);
            var devices = await DeviceInformation.FindAllAsync(selector);
            if (devices.Count == 0)
                throw new Exception("Port not found");

            _deviceInformation = devices[0];
            _device = await SerialDevice.FromIdAsync(_deviceInformation.Id);
            if (_device == null)
                throw new Exception("Unable to connect to device");

            _device.BaudRate = BaudRate;
            _device.DataBits = DataBits;
            _device.StopBits = (WinSerialStopBits)StopBits;
            _device.Parity = (WinSerialParity)Parity;
            _device.IsDataTerminalReadyEnabled = DtrEnable;
            _device.IsRequestToSendEnabled = RtsEnable;
            _device.Handshake = SerialHandshake.None;
            _device.ReadTimeout = TimeSpan.FromMilliseconds(Timeout);
            _device.WriteTimeout = TimeSpan.FromMilliseconds(Timeout);

            _stream = new Stream(_device.InputStream, _device.OutputStream);
        }

        public async Task Close()
        {
            if (_device != null)
            {
                _device.Dispose();

                _device = null;
                _deviceInformation = null;
                _stream = null;
            }
        }

        public void Dispose()
        {
            if (_device != null)
            {
                _device.Dispose();

                _device = null;
                _deviceInformation = null;
                _stream = null;
            }
        }
    }
}
