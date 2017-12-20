/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Brite.Utility.IO;
using Brite.Utility.IO.Serial;
using SerialParity = Brite.Utility.IO.Serial.SerialParity;
using SerialHandshake = Brite.Utility.IO.Serial.SerialHandshake;
using WinSerialStopBits = Windows.Devices.SerialCommunication.SerialStopBitCount;
using WinSerialParity = Windows.Devices.SerialCommunication.SerialParity;
using WinSerialHandshake = Windows.Devices.SerialCommunication.SerialHandshake;

namespace Brite.UWP.Core.IO.Serial
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

        public async Task OpenAsync()
        {
            if (_device != null)
                throw new InvalidOperationException("Port is already open");

            var selector = SerialDevice.GetDeviceSelector(PortName);
            var devices = await DeviceInformation.FindAllAsync(selector);
            if (devices.Count == 0)
                throw new Exception("Port not found");

            _deviceInformation = devices[0];

            var deviceStatus = DeviceAccessInformation.CreateFromId(_deviceInformation.Id).CurrentStatus;
            switch (deviceStatus)
            {
                case DeviceAccessStatus.Unspecified:
                    throw new UnauthorizedAccessException("Port in use");
                case DeviceAccessStatus.DeniedByUser:
                    throw new UnauthorizedAccessException("Port access denied by user");
                case DeviceAccessStatus.DeniedBySystem:
                    throw new UnauthorizedAccessException("Port access denied by system");
            }

            _device = await SerialDevice.FromIdAsync(_deviceInformation.Id);
            if (_device == null)
                throw new Exception("Unable to connect to device");

            _device.BaudRate = BaudRate;
            _device.DataBits = DataBits;
            _device.IsDataTerminalReadyEnabled = DtrEnable;
            _device.IsRequestToSendEnabled = RtsEnable;
            _device.ReadTimeout = TimeSpan.FromMilliseconds(Timeout);
            _device.WriteTimeout = TimeSpan.FromMilliseconds(Timeout);
            _device.Parity = (WinSerialParity)Parity;
            _device.Handshake = (WinSerialHandshake)Handshake;
            _device.StopBits = (WinSerialStopBits)StopBits;

            _stream = new Stream(_device.InputStream, _device.OutputStream);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task CloseAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_device == null)
                throw new InvalidOperationException("Port is not open");

            _device.Dispose();
            _stream.Dispose();

            _device = null;
            _deviceInformation = null;
            _stream = null;
        }

        public void Dispose()
        {
            if (_device != null)
            {
                _device.Dispose();
                _stream.Dispose();

                _device = null;
                _deviceInformation = null;
                _stream = null;
            }
        }
    }
}
