using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Brite.Micro.IO;

namespace Brite.Micro.STKv1
{
    public class StkV1Client : IDisposable
    {
        private const byte CRC_EOP = 0x20;

        private readonly IChannel _port;

        public StkV1Client(IChannel port)
        {
            _port = port;
        }

        public async Task GetSyncLoop()
        {
            const int tries = 3;
            for (var i = 0; i < tries; i++)
            {
                try
                {
                    await GetSynchronization();
                    return;
                }
                catch (StkNoSyncException)
                {
                    if (i == tries - 1) throw;
                }
                catch (TimeoutException)
                {
                    if (i == tries - 1) throw;
                }
            }
            await GetSynchronization();
        }

        public async Task<string> GetSignOn()
        {
            await WriteCommand(StkV1Command.GetSignOn);
            await WriteCrcEop();

            await AssertInSync();
            var res = new List<byte>();
            do
            {
                var ch = await _port.ReceiveByte();
                if (ch == (int)StkV1Response.Ok) break;
                res.Add(ch);
            } while (true);

            return Encoding.ASCII.GetString(res.ToArray());
        }

        public async Task GetSynchronization()
        {
            await WriteCommand(StkV1Command.GetSync);
            await WriteCrcEop();

            await AssertInSync();
            await AssertOk();
        }

        public async Task<byte> GetParameterValue(StkV1Parameter param)
        {
            await WriteCommand(StkV1Command.GetParameterValue);
            await WriteByte((byte)param);
            await WriteCrcEop();

            await AssertInSync();
            var res = await _port.ReceiveByte();
            await AssertOk();
            return res;
        }

        public async Task SetParameterValue(StkV1Parameter param, byte value)
        {
            await WriteCommand(StkV1Command.SetParameterValue);
            await WriteByte((byte)param);
            await WriteByte(value);
            await WriteCrcEop();

            await AssertInSync();
            await AssertOk();
        }

        public async Task SetDeviceParameters(StkV1DeviceParameters parameters)
        {
            await WriteCommand(StkV1Command.SetDeviceParameters);
            await WriteByte((byte)parameters.DeviceCode);
            await WriteByte(parameters.Revision);
            await WriteByte(parameters.ProgType);
            await WriteByte(parameters.ParMode);
            await WriteByte(parameters.Polling);
            await WriteByte(parameters.SelfTimed);
            await WriteByte(parameters.LockBytes);
            await WriteByte(parameters.FuseBytes);
            await WriteByte(parameters.FlashPollVal1);
            await WriteByte(parameters.FlashPollVal2);
            await WriteByte(parameters.EepromPollVal1);
            await WriteByte(parameters.EepromPollVal2);
            await WriteByte(parameters.PageSizeHigh);
            await WriteByte(parameters.PageSizeLow);
            await WriteByte(parameters.EepromSizeHigh);
            await WriteByte(parameters.EepromSizeLow);
            await WriteByte(parameters.FlashSize4);
            await WriteByte(parameters.FlashSize3);
            await WriteByte(parameters.FlashSize2);
            await WriteByte(parameters.FlashSize1);
            await WriteCrcEop();

            await AssertInSync();
            await AssertOk();
        }

        public async Task SetDeviceParametersExt(StkV1DeviceParametersExt parameters)
        {
            await WriteCommand(StkV1Command.SetDeviceParametersExt);
            await WriteByte(5);
            await WriteByte(parameters.EepromPageSize);
            await WriteByte(parameters.SignalPageL);
            await WriteByte(parameters.SignalBs2);
            await WriteByte(parameters.ResetDisable);
            await WriteCrcEop();

            await AssertInSync();
            await AssertOk();
        }

        public async Task EnterProgramMode()
        {
            await WriteCommand(StkV1Command.EnterProgramMode);
            await WriteCrcEop();

            await Task.Delay(200);
            await AssertInSync();
            await AssertOk();
        }

        public async Task LeaveProgramMode()
        {
            await WriteCommand(StkV1Command.LeaveProgramMode);
            await WriteCrcEop();
            await AssertInSync();
            await AssertOk();
        }

        public async Task ChipErase()
        {
            await WriteCommand(StkV1Command.ChipErase);
            await WriteCrcEop();
            await AssertInSync();
            await AssertOk();
        }

        public async Task LoadAddress(ushort val)
        {
            await WriteCommand(StkV1Command.LoadAddress);
            for (var i = 0; i < 2; i++)
            {
                var bt = (byte)(val & 0xff);
                await _port.SendByte(bt);
                val >>= 8;
            }
            await WriteCrcEop();
            await AssertInSync();
            await AssertOk();
        }

        public async Task ProgramFlashMemory(ushort flashCommand)
        {
            await WriteCommand(StkV1Command.ProgramFlashMemory);
            for (var i = 0; i < 2; i++)
            {
                var bt = (byte)(flashCommand & 0xff);
                await _port.SendByte(bt);
                flashCommand >>= 8;
            }
            await WriteCrcEop();
            await AssertInSync();
            await AssertOk();
        }

        public async Task ProgramDataMemory(byte val)
        {
            await WriteCommand(StkV1Command.ProgramDataMemory);
            await WriteByte(val);
            await WriteCrcEop();
            await AssertInSync();
            await AssertOk();
        }

        public async Task ProgramFuseBits(byte fuseBitsLow, byte fuseBitsHigh)
        {
            await WriteCommand(StkV1Command.ProgramFuseBits);
            await _port.SendByte(fuseBitsLow);
            await _port.SendByte(fuseBitsHigh);
            await WriteCrcEop();
            await AssertInSync();
            await AssertOk();
        }

        public async Task ProgramFuseBitsExt(byte fuseBitsLow, byte fuseBitsHigh, byte fuseBitsExt)
        {
            await WriteCommand(StkV1Command.ProgramFuseBitsExt);
            await _port.SendByte(fuseBitsLow);
            await _port.SendByte(fuseBitsHigh);
            await _port.SendByte(fuseBitsExt);
            await WriteCrcEop();
            await AssertInSync();
            await AssertOk();
        }

        public async Task ProgramLockBits(byte lockBits)
        {
            await WriteCommand(StkV1Command.ProgramLockBits);
            await _port.SendByte(lockBits);
            await WriteCrcEop();
            await AssertInSync();
            await AssertOk();
        }

        public async Task ProgramPage(byte[] data, int dataStart, int dataLength, MemoryType memType)
        {
            await WriteCommand(StkV1Command.ProgramPage);
            await WriteByte((byte)(dataLength >> 8));
            await WriteByte((byte)(dataLength & 0xff));
            switch (memType)
            {
                case MemoryType.Flash:
                    await WriteChar('F');
                    break;
                case MemoryType.Eeprom:
                    await WriteChar('E');
                    break;
                default:
                    throw new NotSupportedException();
            }
            for (var i = 0; i < dataLength; i++)
            {
                await _port.SendByte(data[i + dataStart]);
            }
            await WriteCrcEop();
            await AssertInSync();
            await AssertOk();
        }

        public async Task ReadFlashMemory(byte[] data, int dataStart, int dataLength)
        {
            await WriteCommand(StkV1Command.ReadFlashMemory);
            await WriteByte((byte)(dataLength >> 8));
            await WriteByte((byte)(dataLength & 0xff));
            await WriteCrcEop();
            await AssertInSync();

            for (var i = 0; i < dataLength; i++)
            {
                data[i + dataStart] = await _port.ReceiveByte();
            }
            await AssertOk();
        }

        public async Task ReadDataMemory(byte[] data, int dataStart, int dataLength)
        {
            await WriteCommand(StkV1Command.ReadDataMemory);
            await WriteByte((byte)(dataLength >> 8));
            await WriteByte((byte)(dataLength & 0xff));
            await WriteCrcEop();
            await AssertInSync();

            for (var i = 0; i < dataLength; i++)
            {
                data[i + dataStart] = await _port.ReceiveByte();
            }
            await AssertOk();
        }

        public async Task<StkFuseBits> ReadFuseBits()
        {
            await WriteCommand(StkV1Command.ReadFuseBits);
            await WriteCrcEop();
            await AssertInSync();

            StkFuseBits res;
            res.Low = await _port.ReceiveByte();
            res.High = await _port.ReceiveByte();
            await AssertOk();

            return res;
        }

        public async Task<StkFuseBitsExt> ReadFuseBitsExt()
        {
            await WriteCommand(StkV1Command.ReadFuseBitsExt);
            await WriteCrcEop();
            await AssertInSync();

            StkFuseBitsExt res;
            res.Low = await _port.ReceiveByte();
            res.High = await _port.ReceiveByte();
            res.Extended = await _port.ReceiveByte();
            await AssertOk();

            return res;
        }

        public async Task<byte> ReadLockBits()
        {
            await WriteCommand(StkV1Command.ReadLockBits);
            await WriteCrcEop();
            await AssertInSync();

            var res = await _port.ReceiveByte();
            await AssertOk();

            return res;
        }

        public async Task ReadPage(byte[] data, int dataStart, int dataLength, MemoryType memType)
        {
            await WriteCommand(StkV1Command.ReadPage);
            await WriteByte((byte)(dataLength >> 8));
            await WriteByte((byte)(dataLength & 0xff));
            switch (memType)
            {
                case MemoryType.Flash:
                    await WriteChar('F');
                    break;
                case MemoryType.Eeprom:
                    await WriteChar('E');
                    break;
                default:
                    throw new NotSupportedException();
            }
            await WriteCrcEop();
            await AssertInSync();

            for (var i = 0; i < dataLength; i++)
            {
                data[i + dataStart] = await _port.ReceiveByte();
            }
            await AssertOk();
        }

        public async Task<Signature> ReadSignatureBytes()
        {
            await WriteCommand(StkV1Command.ReadSignatureBytes);
            await WriteCrcEop();
            await AssertInSync();

            Signature res;
            res.Vendor = await _port.ReceiveByte();
            res.Middle = await _port.ReceiveByte();
            res.Low = await _port.ReceiveByte();
            await AssertOk();

            return res;
        }

        public async Task<byte> ReadOscillatorCalibrationByte()
        {
            await WriteCommand(StkV1Command.ReadOscillatorCalibrationByte);
            await WriteCrcEop();
            await AssertInSync();

            var res = await _port.ReceiveByte();
            await AssertOk();

            return res;
        }

        public async Task<byte> ReadOscillatorCalibrationByteExt(byte adr)
        {
            await WriteCommand(StkV1Command.ReadOscillatorCalibrationByteExt);
            await _port.SendByte(adr);
            await WriteCrcEop();
            await AssertInSync();

            var res = await _port.ReceiveByte();
            await AssertOk();

            return res;
        }

        public async Task<byte> Universal(byte a, byte b, byte c, byte d)
        {
            await WriteCommand(StkV1Command.Universal);
            await _port.SendByte(a);
            await _port.SendByte(b);
            await _port.SendByte(c);
            await _port.SendByte(d);
            await WriteCrcEop();

            await AssertInSync();
            var res = await _port.ReceiveByte();
            await AssertOk();

            return res;
        }

        public async Task<byte> UniversalExt(byte[] cmd)
        {
            await WriteCommand(StkV1Command.UniversalExt);
            await _port.SendByte((byte)(cmd.Length - 1));
            foreach (var bt in cmd)
            {
                await _port.SendByte(bt);
            }
            await WriteCrcEop();

            await AssertInSync();
            var res = await _port.ReceiveByte();
            await AssertOk();

            return res;
        }

        public async Task ResetDevice()
        {
            await _port.ToggleReset(true);
            await Task.Delay(200);
            await _port.ToggleReset(false);
            await Task.Delay(200);
            await _port.ToggleReset(true);
            await Task.Delay(200); //in-circuit capacity will break dtr signal. so we can skip dtr=true
        }

        public async Task Open()
        {
            await _port.Open();
        }


        public async Task<StkVersion> ReadVersion()
        {
            return new StkVersion
            {
                Hardware = await GetParameterValue(StkV1Parameter.HardwareVersion),
                SoftwareMajor = await GetParameterValue(StkV1Parameter.SoftwareMajorVersion),
                SoftwareMinor = await GetParameterValue(StkV1Parameter.SoftwareMinorVersion)
            };
        }

        public async Task Close()
        {
            await _port.Close();
        }


        private async Task WriteChar(char ch)
        {
            await WriteByte((byte)ch);
        }

        private async Task WriteByte(byte bt)
        {
            await _port.SendByte(bt);
        }

        private async Task WriteCommand(StkV1Command command)
        {
            await _port.SendByte((byte)command);
        }

        public void Dispose()
        {
            _port.Dispose();
        }

        private async Task AssertInSync()
        {
            var ch = await _port.ReceiveByte();
            if (ch != (int)StkV1Response.InSync) throw new StkNoSyncException();
        }

        private async Task AssertOk()
        {
            var ch = await _port.ReceiveByte();
            if (ch != (int)StkV1Response.Ok) throw new StkFailedException();
        }

        private async Task WriteCrcEop()
        {
            await _port.SendByte(CRC_EOP);
        }
    }
}
