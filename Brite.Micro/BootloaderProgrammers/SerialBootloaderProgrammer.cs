using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brite.Micro.Hardware;
using Brite.Micro.Hardware.Memory;
using Brite.Micro.Intel;
using Brite.Micro.Protocols;
using Brite.Utility.IO;

namespace Brite.Micro.BootloaderProgrammers
{
    public abstract class SerialBootloaderProgrammer<TSerialConnection> : BootloaderProgrammer where TSerialConnection : ISerialConnection, new()
    {
        private static readonly Log log = Logger.GetLog<SerialBootloaderProgrammer<TSerialConnection>>();

        private readonly TSerialConnection _serial;
        private readonly SerialConfig _config;

        protected SerialBootloaderProgrammer(SerialConfig config, IMicrocontroller mcu)
            : base(mcu)
        {
            _config = config;
            _serial = new TSerialConnection();
        }

        public override async Task Open()
        {
            _serial.PortName = _config.PortName;
            _serial.BaudRate = (uint)_config.BaudRate;
            _serial.Timeout = _config.ReadTimeOut;
            _serial.DtrEnable = true;

            await _serial.Open();

            log.Trace("Opened serial port {0} with baud rate {1}!", _config.PortName, _config.BaudRate);
        }

        public override async Task Close()
        {
            log.Trace("Closing port");

            _serial.DtrEnable = false;
            _serial.RtsEnable = false;
            await _serial.Close();
        }

        protected async Task ToggleDtrRts(int wait1, int wait2, bool invert = false)
        {
            log.Trace("Toggling DTR/RTS...");

            _serial.DtrEnable = invert;
            _serial.RtsEnable = invert;

            await Task.Delay(wait1);

            _serial.DtrEnable = !invert;
            _serial.RtsEnable = !invert;

            await Task.Delay(wait2);
        }

        protected virtual async Task Send(IRequest request)
        {
            var bytes = request.Bytes;
            var length = bytes.Length;

            log.Debug("Sending {0} bytes: {1}{2}", length, Environment.NewLine, BitConverter.ToString(bytes));

            await _serial.BaseStream.WriteAsync(bytes, 0, length);
        }

        protected async Task<TResponse> Receive<TResponse>(int length = 1) where TResponse : Response, new()
        {
            var bytes = await ReceiveNext(length);
            return bytes == null ? null : new TResponse { Bytes = bytes };
        }

        protected async Task<int> ReceiveNext()
        {
            var bytes = new byte[1];
            try
            {
                if (await _serial.BaseStream.ReadAsync(bytes, 0, 1) == 0)
                    throw new TimeoutException();
                log.Debug("Receiving byte: {0}", bytes[0]);
                return bytes[0];
            }
            catch (TimeoutException) // TODO/NOTE: Test if TimeoutException even works on our streams
            {
                return -1;
            }
        }

        protected async Task<byte[]> ReceiveNext(int length)
        {
            var bytes = new byte[length];
            var retrieved = 0;
            try
            {
                while (retrieved < length)
                {
                    var readCount = await _serial.BaseStream.ReadAsync(bytes, retrieved, length - retrieved);
                    if (readCount == 0)
                        throw new TimeoutException();
                    retrieved += readCount;
                }
                log.Debug("Receiving bytes: {0}", Encoding.ASCII.GetString(bytes));
                return bytes;
            }
            catch (TimeoutException)
            {
                return null;
            }
        }
    }
}
