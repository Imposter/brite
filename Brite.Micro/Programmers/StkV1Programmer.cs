/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

/* 
 * MIT License
 *
 * Copyright (C) 2017 Sergey Savchuk. All Rights Reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 */

using System;
using System.Threading.Tasks;
using Brite.Micro.Programmers.StkV1;
using Brite.Micro.Programmers.StkV1.Protocol;
using Brite.Utility.IO;

namespace Brite.Micro.Programmers
{
    // For more information, see: http://www.atmel.com/images/doc2525.pdf
    // Based on: https://github.com/SavchukSergey/Flasher
    public class StkV1Programmer : SerialProgrammer
    {
        public const int DefaultSyncRetries = 5;
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

        public override async Task OpenAsync()
        {
            await base.OpenAsync();
            if (_reset)
                await ResetAsync();

            await WaitAsync();
            await SetDeviceParametersAsync(new DeviceParameters
            {
                DeviceCode = _info.Type,
                Revision = 0,
                ProgramType = 0,
                ParallelMode = 1,
                Polling = 1,
                SelfTimed = 1,
                LockBytes = (byte)_info.LockBits.Size,
                FuseBytes = (byte)_info.LockBits.Size,
                FlashPollValue = 0xFF,
                EepromPollValue = 0xFF,
                PageSize = (ushort)_info.Flash.PageSize,
                EepromSize = (ushort)_info.Eeprom.Size,
                FlashSize = (uint)_info.Flash.Size
            });
            await SetDeviceParametersExtAsync(new DeviceParametersExt
            {
                EepromPageSize = (byte)_info.Eeprom.PageSize,
                SignalPageL = 0xD7, // Port D7
                SignalBs2 = 0xC2, // Port C2
                ResetDisable = 0
            });
            await EnterProgramModeAsync();
        }

        public override async Task CloseAsync()
        {
            await LeaveProgramModeAsync();
            await base.CloseAsync();
        }
        
        public override async Task ReadAsync(MemoryType type, byte[] data, int offset, int length)
        {
            switch (type)
            {
                case MemoryType.Flash:
                case MemoryType.Eeprom:
                    var position = 0;
                    var pageSize = type == MemoryType.Flash ? _info.Flash.PageSize : _info.Eeprom.PageSize;
                    while (position < length)
                    {
                        await LoadAddressAsync((ushort)(position / 2));
                        var count = Math.Min(length - position, pageSize);
                        await ReadPageAsync(type, data, position + offset, count);
                        position += count;
                    }
                    break;
                case MemoryType.LockBits:
                    for (var i = 0; i < length; i++)
                        data[i + offset] = await ReadLockByteAsync(i);
                    break;
                case MemoryType.FuseBits:
                    for (var i = 0; i < length; i++)
                        data[i + offset] = await ReadFuseByteAsync(i);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        public override async Task WriteAsync(MemoryType type, byte[] data, int offset, int length)
        {
            switch (type)
            {
                case MemoryType.Flash:
                case MemoryType.Eeprom:
                    var position = 0;
                    var pageSize = type == MemoryType.Flash ? _info.Flash.PageSize : _info.Eeprom.PageSize;
                    while (position < length)
                    {
                        await LoadAddressAsync((ushort)(position / 2));
                        var count = Math.Min(length - position, pageSize);
                        await ProgramPageAsync(type, data, position + offset, count);
                        position += count;
                    }
                    break;
                case MemoryType.LockBits:
                    for (var i = 0; i < length; i++)
                        await WriteLockByteAsync(i, data[i + offset]);
                    break;
                case MemoryType.FuseBits:
                    for (var i = 0; i < length; i++)
                        await WriteFuseByteAsync(i, data[i + offset]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        public override async Task EraseAsync()
        {
            await ChipEraseAsync();
        }

        // TODO/NOTE: The timings in this aren't 100% reliable, it may reset too early and fail to synchronize with the device
        public override async Task ResetAsync()
        {
            // Turn off DTR
            await Channel.ToggleResetAsync(false);
            await Task.Delay(200);

            // Turn on DTR, this will cause the device to reset
            await Channel.ToggleResetAsync(true);
            await Task.Delay(200);
        }

        #region Protocol

        private async Task WaitAsync()
        {
            for (var i = 0; i < _retries; i++)
            {
                try
                {
                    await SynchronizeAsync();
                }
                catch
                {
                    if (i == _retries - 1)
                        throw;
                }
            }
        }

        private async Task SynchronizeAsync()
        {
            await SendRequestAsync(Command.GetSync);
        }

        private async Task SetDeviceParametersAsync(DeviceParameters parameters)
        {
            await SendRequestAsync(Command.SetDeviceParameters, async stream =>
            {
                await stream.WriteUInt8Async((byte)parameters.DeviceCode);
                await stream.WriteUInt8Async(parameters.Revision);
                await stream.WriteUInt8Async(parameters.ProgramType);
                await stream.WriteUInt8Async(parameters.ParallelMode);
                await stream.WriteUInt8Async(parameters.Polling);
                await stream.WriteUInt8Async(parameters.SelfTimed);
                await stream.WriteUInt8Async(parameters.LockBytes);
                await stream.WriteUInt8Async(parameters.FuseBytes);
                await stream.WriteUInt8Async(parameters.FlashPollValue);
                await stream.WriteUInt8Async(parameters.FlashPollValue);
                await stream.WriteUInt8Async(parameters.EepromPollValue);
                await stream.WriteUInt8Async(parameters.EepromPollValue);

                stream.BigEndian = true;
                await stream.WriteUInt16Async(parameters.PageSize);
                await stream.WriteUInt16Async(parameters.EepromSize);
                await stream.WriteUInt32Async(parameters.FlashSize);
            });
        }

        private async Task SetDeviceParametersExtAsync(DeviceParametersExt parameters)
        {
            await SendRequestAsync(Command.SetDeviceParametersExt, async stream =>
            {
                // Amount of parameters
                await stream.WriteUInt8Async(4);

                await stream.WriteUInt8Async(parameters.EepromPageSize);
                await stream.WriteUInt8Async(parameters.SignalPageL);
                await stream.WriteUInt8Async(parameters.SignalBs2);
                await stream.WriteUInt8Async(parameters.ResetDisable);
            });
        }

        private async Task EnterProgramModeAsync()
        {
            await SendRequestAsync(Command.EnterProgramMode);
        }

        private async Task LeaveProgramModeAsync()
        {
            await SendRequestAsync(Command.LeaveProgramMode);
        }

        private async Task ChipEraseAsync()
        {
            await SendRequestAsync(Command.ChipErase);
        }

        private async Task<byte> UniversalAsync(byte a, byte b, byte c, byte d)
        {
            byte value = 0;
            await SendRequestAsync(Command.Universal, async stream =>
            {
                await stream.WriteUInt8Async(a);
                await stream.WriteUInt8Async(b);
                await stream.WriteUInt8Async(c);
                await stream.WriteUInt8Async(d);
            }, async (channel, stream) =>
            {
                value = await stream.ReadUInt8Async();
            });
            return value;
        }

        private async Task LoadAddressAsync(ushort value)
        {
            await SendRequestAsync(Command.LoadAddress, async stream =>
            {
                await stream.WriteUInt16Async(value);
            });
        }

        private async Task ProgramPageAsync(MemoryType memoryType, byte[] data, int offset, int length)
        {
            await SendRequestAsync(Command.ProgramPage, async stream =>
            {
                stream.BigEndian = true;
                await stream.WriteUInt16Async((ushort)length);
                switch (memoryType)
                {
                    case MemoryType.Flash:
                        await stream.WriteUInt8Async((byte)'F');
                        break;
                    case MemoryType.Eeprom:
                        await stream.WriteUInt8Async((byte)'E');
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(memoryType), memoryType, null);
                }

                for (var i = 0; i < length; i++)
                    await stream.WriteUInt8Async(data[offset + i]);
            });
        }

        private async Task ReadPageAsync(MemoryType memoryType, byte[] data, int offset, int length)
        {
            await SendRequestAsync(Command.ReadPage, async stream =>
            {
                stream.BigEndian = true;
                await stream.WriteUInt16Async((ushort)length);
                switch (memoryType)
                {
                    case MemoryType.Flash:
                        await stream.WriteUInt8Async((byte)'F');
                        break;
                    case MemoryType.Eeprom:
                        await stream.WriteUInt8Async((byte)'E');
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(memoryType), memoryType, null);
                }
            }, async (channel, stream) =>
            {
                for (var i = 0; i < length; i++)
                    data[offset + i] = await stream.ReadUInt8Async();
            });
        }

        private async Task<byte> ReadLockByteAsync(int address)
        {
            if (address == 0)
                return await UniversalAsync(0x58, 0x00, 0x00, 0x00);
            return 0;
        }

        private async Task WriteLockByteAsync(int address, byte value)
        {
            if (address == 0)
                await UniversalAsync(0xAC, 0xE0, 0x00, value);
        }

        private async Task<byte> ReadFuseByteAsync(int address)
        {
            switch (address)
            {
                case 0:
                    return await UniversalAsync(0x50, 0x00, 0x00, 0x00);
                case 1:
                    return await UniversalAsync(0x58, 0x08, 0x00, 0x00);
                case 2:
                    return await UniversalAsync(0x50, 0x08, 0x00, 0x00);
                default:
                    throw new ArgumentOutOfRangeException(nameof(address), address, null);
            }
        }

        private async Task WriteFuseByteAsync(int address, byte value)
        {
            switch (address)
            {
                case 0:
                    await UniversalAsync(0xAC, 0xA0, 0x00, value);
                    break;
                case 1:
                    await UniversalAsync(0xAC, 0xA8, 0x00, value);
                    break;
                case 2:
                    await UniversalAsync(0xAC, 0xA4, 0x00, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(address), address, null);
            }
        }

        private delegate Task RequestCallback(BinaryStream stream);
        private delegate Task ResponseCallback(SerialChannel channel, BinaryStream stream);
        private async Task SendRequestAsync(Command command, RequestCallback requestCallback, ResponseCallback responseCallback)
        {
            // Write request
            Channel.Stream.BigEndian = false;
            await Channel.Stream.WriteUInt8Async((byte)command);
            if (requestCallback != null)
                await requestCallback(Channel.Stream);

            Channel.Stream.BigEndian = false;
            await Channel.Stream.WriteUInt8Async(CrcEop);

            // Check if synchronized
            if (await Channel.Stream.ReadUInt8Async() != (byte)Result.InSync)
                throw new ProtocolException("Not synchronized");

            // Read response
            if (responseCallback != null)
                await responseCallback(Channel, Channel.Stream);

            Channel.Stream.BigEndian = false;
            if (await Channel.Stream.ReadUInt8Async() != (byte)Result.Ok)
                throw new ProtocolException("Request failed");
        }

        private async Task SendRequestAsync(Command command, RequestCallback requestCallback)
        {
            await SendRequestAsync(command, requestCallback, null);
        }

        private async Task SendRequestAsync(Command command, ResponseCallback responseCallback)
        {
            await SendRequestAsync(command, null, responseCallback);
        }

        private async Task SendRequestAsync(Command command)
        {
            await SendRequestAsync(command, null, null);
        }

        #endregion
    }
}
