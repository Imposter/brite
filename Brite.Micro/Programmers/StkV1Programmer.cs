using System;
using System.Threading.Tasks;
using Brite.Micro.Programmers.StkV1;
using Brite.Micro.Programmers.StkV1.Protocol;
using Brite.Utility.IO;
using DeviceInfo = Brite.Micro.Programmers.StkV1.DeviceInfo;

namespace Brite.Micro.Programmers
{
    // For more documentation, see: http://www.atmel.com/images/doc2525.pdf
    public class StkV1Programmer : SerialProgrammer
    {
        public const int DefaultSyncRetries = 5;
        private const int BlockSize = 1024;
        private const byte CrcEop = 0x20;

        private readonly DeviceInfo _info;
        private readonly bool _reset;
        private readonly int _retries;

        public StkV1Programmer(Channel channel, DeviceInfo info, bool reset = true, int retries = DefaultSyncRetries)
            : base(channel)
        {
            _info = info;
            _reset = reset;
            _retries = retries;
        }

        public override async Task Open()
        {
            await base.Open();
            if (_reset)
                await ResetDevice();

            await WaitForSync();
            await SetDeviceParameters(new DeviceParameters
            {
                DeviceCode = _info.Type,
                Revision = 0,
                ProgType = 0,
                ParMode = 1,
                Polling = 1,
                SelfTimed = 1,
                LockBytes = 1,
                FuseBytes = 3,
                FlashPollVal1 = 0xFF,
                FlashPollVal2 = 0xFF,
                EepromPollVal1 = 0xFF,
                EepromPollVal2 = 0xFF,
                PageSize = (ushort)_info.Flash.PageSize,
                EepromSize = (ushort)_info.Eeprom.Size,
                FlashSize = (uint)_info.Flash.Size
            });
            await SetDeviceParametersExt(new DeviceParametersExt
            {
                EepromPageSize = (byte)_info.Eeprom.PageSize,
                SignalPageL = 0xD7, // Port D7
                SignalBs2 = 0xC2, // Port C2
                ResetDisable = 0
            });
            await EnterProgramMode();
        }

        public override async Task Close()
        {
            await LeaveProgramMode();
            await base.Close();
        }
        
        public override async Task ReadPage(MemoryType type, int address, byte[] data, int offset, int length)
        {
            switch (type)
            {
                case MemoryType.Flash:
                case MemoryType.Eeprom:
                    var position = address;
                    var end = address + length;
                    while (position < end)
                    {
                        await LoadAddress((ushort)(position >> 1));
                        var count = Math.Min(end - position, BlockSize);
                        await ReadPage(type, data, position - address + offset, count);
                        position += count;
                    }
                    break;
                case MemoryType.LockBits:
                    for (var i = 0; i < length; i++)
                        data[i + offset] = await ReadLockByte(address + i);
                    break;
                case MemoryType.FuseBits:
                    for (var i = 0; i < length; i++)
                        data[i + offset] = await ReadFuseByte(address + i);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        public override async Task WritePage(MemoryType type, int address, byte[] data, int offset, int length)
        {
            switch (type)
            {
                case MemoryType.Flash:
                case MemoryType.Eeprom:
                    var position = address;
                    var end = address + length;
                    while (position < end)
                    {
                        await LoadAddress((ushort)(position >> 1));
                        var count = Math.Min(end - position, BlockSize);
                        await ProgramPage(type, data, position - address + offset, count);
                        position += count;
                    }
                    break;
                case MemoryType.LockBits:
                    for (var i = 0; i < length; i++)
                        await WriteLockByte(address + i, data[i + offset]);
                    break;
                case MemoryType.FuseBits:
                    for (var i = 0; i < length; i++)
                        await WriteFuseByte(address + i, data[i + offset]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private async Task ResetDevice()
        {
            // Turn off DTR
            await Channel.ToggleReset(false);
            await Task.Delay(200);

            // Turn on DTR, this will cause the device to reset
            await Channel.ToggleReset(true);
            await Task.Delay(200);
        }

        #region Protocol

        private async Task WaitForSync()
        {
            for (var i = 0; i < _retries; i++)
            {
                try
                {
                    await GetSync();
                }
                catch
                {
                    if (i == _retries - 1)
                        throw;
                }
            }
        }

        private async Task GetSync()
        {
            await SendRequest(Command.GetSync);
        }

        private async Task SetDeviceParameters(DeviceParameters parameters)
        {
            await SendRequest(Command.SetDeviceParameters, async stream =>
            {
                await stream.WriteUInt8((byte)parameters.DeviceCode);
                await stream.WriteUInt8(parameters.Revision);
                await stream.WriteUInt8(parameters.ProgType);
                await stream.WriteUInt8(parameters.ParMode);
                await stream.WriteUInt8(parameters.Polling);
                await stream.WriteUInt8(parameters.SelfTimed);
                await stream.WriteUInt8(parameters.LockBytes);
                await stream.WriteUInt8(parameters.FuseBytes);
                await stream.WriteUInt8(parameters.FlashPollVal1);
                await stream.WriteUInt8(parameters.FlashPollVal2);
                await stream.WriteUInt8(parameters.EepromPollVal1);
                await stream.WriteUInt8(parameters.EepromPollVal2);

                stream.BigEndian = true;
                await stream.WriteUInt16(parameters.PageSize);
                await stream.WriteUInt16(parameters.EepromSize);
                await stream.WriteUInt32(parameters.FlashSize);
            });
        }

        private async Task SetDeviceParametersExt(DeviceParametersExt parameters)
        {
            await SendRequest(Command.SetDeviceParametersExt, async stream =>
            {
                // Amount of parameters
                await stream.WriteUInt8(4);

                await stream.WriteUInt8(parameters.EepromPageSize);
                await stream.WriteUInt8(parameters.SignalPageL);
                await stream.WriteUInt8(parameters.SignalBs2);
                await stream.WriteUInt8(parameters.ResetDisable);
            });
        }

        private async Task EnterProgramMode()
        {
            await SendRequest(Command.EnterProgramMode);
        }

        private async Task LeaveProgramMode()
        {
            await SendRequest(Command.LeaveProgramMode);
        }

        private async Task<byte> Universal(byte a, byte b, byte c, byte d)
        {
            byte value = 0;
            await SendRequest(Command.Universal, async stream =>
            {
                await stream.WriteUInt8(a);
                await stream.WriteUInt8(b);
                await stream.WriteUInt8(c);
                await stream.WriteUInt8(d);
            }, async (channel, stream) =>
            {
                value = await stream.ReadUInt8();
            });
            return value;
        }

        private async Task LoadAddress(ushort value)
        {
            await SendRequest(Command.LoadAddress, async stream =>
            {
                await stream.WriteUInt16(value);
            });
        }

        private async Task ProgramPage(MemoryType memoryType, byte[] data, int offset, int length)
        {
            await SendRequest(Command.ProgramPage, async stream =>
            {
                stream.BigEndian = true;
                await stream.WriteUInt16((ushort)length);
                switch (memoryType)
                {
                    case MemoryType.Flash:
                        await stream.WriteUInt8((byte)'F');
                        break;
                    case MemoryType.Eeprom:
                        await stream.WriteUInt8((byte)'E');
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(memoryType), memoryType, null);
                }

                for (var i = 0; i < length; i++)
                    await stream.WriteUInt8(data[offset + i]);
            });
        }

        private async Task ReadPage(MemoryType memoryType, byte[] data, int offset, int length)
        {
            await SendRequest(Command.ReadPage, async stream =>
            {
                stream.BigEndian = true;
                await stream.WriteUInt16((ushort)length);
                switch (memoryType)
                {
                    case MemoryType.Flash:
                        await stream.WriteUInt8((byte)'F');
                        break;
                    case MemoryType.Eeprom:
                        await stream.WriteUInt8((byte)'E');
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(memoryType), memoryType, null);
                }
            }, async (channel, stream) =>
            {
                for (var i = 0; i < length; i++)
                    data[offset + i] = await stream.ReadUInt8();
            });
        }

        private async Task<byte> ReadLockByte(int address)
        {
            if (address == 0)
                return await Universal(0x58, 0x00, 0x00, 0x00);
            return 0;
        }

        private async Task WriteLockByte(int address, byte value)
        {
            if (address == 0)
                await Universal(0xAC, 0xE0, 0x00, value);
        }

        private async Task<byte> ReadFuseByte(int address)
        {
            switch (address)
            {
                case 0:
                    return await Universal(0x50, 0x00, 0x00, 0x00);
                case 1:
                    return await Universal(0x58, 0x08, 0x00, 0x00);
                case 2:
                    return await Universal(0x50, 0x08, 0x00, 0x00);
                default:
                    return 0;
            }
        }

        private async Task WriteFuseByte(int address, byte value)
        {
            switch (address)
            {
                case 0:
                    await Universal(0xAC, 0xA0, 0x00, value);
                    break;
                case 1:
                    await Universal(0xAC, 0xA8, 0x00, value);
                    break;
                case 2:
                    await Universal(0xAC, 0xA4, 0x00, value);
                    break;
            }
        }

        private delegate Task RequestCallback(BinaryStream stream);
        private delegate Task ResponseCallback(SerialChannel channel, BinaryStream stream);
        private async Task SendRequest(Command command, RequestCallback requestCallback, ResponseCallback responseCallback)
        {
            // Write request
            Channel.Stream.BigEndian = false;
            await Channel.Stream.WriteUInt8((byte)command);
            if (requestCallback != null)
                await requestCallback(Channel.Stream);

            Channel.Stream.BigEndian = false;
            await Channel.Stream.WriteUInt8(CrcEop);

            // Check if synchronized
            if (await Channel.Stream.ReadUInt8() != (byte)Result.InSync)
                throw new ProtocolException("Not synchronized");

            // Read response
            if (responseCallback != null)
                await responseCallback(Channel, Channel.Stream);

            Channel.Stream.BigEndian = false;
            if (await Channel.Stream.ReadUInt8() != (byte)Result.Ok)
                throw new ProtocolException("Request failed");
        }

        private async Task SendRequest(Command command, RequestCallback requestCallback)
        {
            await SendRequest(command, requestCallback, null);
        }

        private async Task SendRequest(Command command, ResponseCallback responseCallback)
        {
            await SendRequest(command, null, responseCallback);
        }

        private async Task SendRequest(Command command)
        {
            await SendRequest(command, null, null);
        }

        #endregion
    }
}
