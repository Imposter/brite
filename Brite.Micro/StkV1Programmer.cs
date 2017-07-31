using System;
using System.Threading.Tasks;
using Brite.Micro.Devices;
using Brite.Micro.STKv1;

namespace Brite.Micro
{
    public class StkV1Programmer : IProgrammer
    {
        private readonly StkV1Client _client;
        private readonly DeviceInfo _device;
        private readonly bool _useReset;
        private const int BLOCK_SIZE = 1024;

        public StkV1Programmer(StkV1Client client, DeviceInfo device, bool useReset)
        {
            _client = client;
            _device = device;
            _useReset = useReset;
        }

        public async Task<ProgrammingSession> Start()
        {
            await _client.Open();
            if (_useReset)
            {
                await _client.ResetDevice();
            }
            else
            {
                await Task.Delay(1500);
            }
            await _client.GetSyncLoop();
            await _client.SetDeviceParameters(new StkV1DeviceParameters
            {
                DeviceCode = _device.StkCode,
                Revision = 0,
                ProgType = 0,
                ParMode = 1,
                Polling = 1,
                SelfTimed = 1,
                LockBytes = 1,
                FuseBytes = 3,
                FlashPollVal1 = 0xff,
                FlashPollVal2 = 0xff,
                EepromPollVal1 = 0xff,
                EepromPollVal2 = 0xff,
                PageSize = (ushort)_device.Flash.PageSize,
                EepromSize = (ushort)_device.Eeprom.Size,
                FlashSize = (uint)_device.Flash.Size
            });
            await _client.SetDeviceParametersExt(new StkV1DeviceParametersExt
            {
                EepromPageSize = (byte)_device.Eeprom.PageSize,
                SignalPageL = 0xd7,
                SignalBs2 = 0xc2,
                ResetDisable = 0
            });
            await _client.EnterProgramMode();
            return new ProgrammingSession(this);
        }

        public async Task Stop()
        {
            await _client.LeaveProgramMode();
            await _client.Close();
        }

        public async Task ReadPage(int address, MemoryType memType, byte[] data, int dataStart, int dataLength)
        {
            switch (memType)
            {
                case MemoryType.Eeprom:
                    await ReadEeprom(address, data, dataStart, dataLength);
                    break;
                case MemoryType.Flash:
                    await ReadFlash(address, data, dataStart, dataLength);
                    break;
                case MemoryType.LockBits:
                    for (var i = 0; i < dataLength; i++)
                    {
                        data[i + dataStart] = await ReadLockByte(address + i);
                    }
                    break;
                case MemoryType.FuseBits:
                    for (var i = 0; i < dataLength; i++)
                    {
                        data[i + dataStart] = await ReadFuseByte(address + i);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public async Task WritePage(int address, MemoryType memType, byte[] data, int dataStart, int dataLength)
        {
            switch (memType)
            {
                case MemoryType.Eeprom:
                    await WriteEeprom(address, data, dataStart, dataLength);
                    break;
                case MemoryType.Flash:
                    await WriteFlash(address, data, dataStart, dataLength);
                    break;
                case MemoryType.LockBits:
                    for (var i = 0; i < dataLength; i++)
                    {
                        await WriteLockByte(address + i, data[i + dataStart]);
                    }
                    break;
                case MemoryType.FuseBits:
                    for (var i = 0; i < dataLength; i++)
                    {
                        await WriteFuseByte(address + i, data[i + dataStart]);
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private async Task<byte> ReadLockByte(int address)
        {
            switch (address)
            {
                case 0:
                    return await _client.Universal(0x58, 0x00, 0x00, 0x00);
                default:
                    return 0;
            }
        }

        private async Task WriteLockByte(int address, byte val)
        {
            switch (address)
            {
                case 0:
                    await _client.Universal(0xac, 0xe0, 0x00, val);
                    break;
            }
        }

        private async Task<byte> ReadFuseByte(int address)
        {
            switch (address)
            {
                case 0:
                    return await _client.Universal(0x50, 0x00, 0x00, 0x00);
                case 1:
                    return await _client.Universal(0x58, 0x08, 0x00, 0x00);
                case 2:
                    return await _client.Universal(0x50, 0x08, 0x00, 0x00);
                default:
                    return 0;
            }
        }

        private async Task WriteFuseByte(int address, byte val)
        {
            switch (address)
            {
                case 0:
                    await _client.Universal(0xac, 0xa0, 0x00, val);
                    break;
                case 1:
                    await _client.Universal(0xac, 0xa8, 0x00, val);
                    break;
                case 2:
                    await _client.Universal(0xac, 0xa4, 0x00, val);
                    break;
            }
        }

        public async Task EraseDevice()
        {
            await _client.Universal(0xac, 0x80, 0x00, 0x00);
        }

        private async Task WriteEeprom(int address, byte[] data, int dataStart, int dataLength)
        {
            var offset = address;
            var end = address + dataLength;
            while (offset < end)
            {
                await _client.LoadAddress((ushort)(offset >> 1));
                var cnt = Math.Min(end - offset, BLOCK_SIZE);

                await _client.ProgramPage(data, offset - address + dataStart, cnt, MemoryType.Eeprom);
                offset += cnt;
            }
        }

        private async Task ReadEeprom(int address, byte[] data, int dataStart, int dataLength)
        {
            var offset = address;
            var end = address + dataLength;
            while (offset < end)
            {
                await _client.LoadAddress((ushort)(offset >> 1));
                var cnt = Math.Min(end - offset, BLOCK_SIZE);
                await _client.ReadPage(data, offset - address + dataStart, cnt, MemoryType.Eeprom);
                offset += cnt;
            }
        }

        private async Task WriteFlash(int start, byte[] data, int dataStart, int dataLength)
        {
            var offset = start;
            var end = start + dataLength;
            while (offset < end)
            {
                await _client.LoadAddress((ushort)(offset >> 1));
                var cnt = Math.Min(end - offset, BLOCK_SIZE);

                await _client.ProgramPage(data, offset - start + dataStart, cnt, MemoryType.Flash);
                offset += cnt;
            }
        }

        private async Task ReadFlash(int address, byte[] data, int dataStart, int dataLength)
        {
            var offset = address;
            var end = address + dataLength;
            while (offset < end)
            {
                await _client.LoadAddress((ushort)(offset >> 1));
                var cnt = Math.Min(end - offset, BLOCK_SIZE);
                await _client.ReadPage(data, offset - address + dataStart, cnt, MemoryType.Flash);
                offset += cnt;
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
