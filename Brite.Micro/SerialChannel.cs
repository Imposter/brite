/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Threading.Tasks;
using Brite.Utility.IO;
using Brite.Utility.IO.Serial;

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

        public async Task OpenAsync()
        {
            await _serial.OpenAsync();
            _stream = new BinaryStream(_serial.BaseStream);
        }

        public async Task CloseAsync()
        {
            await _serial.CloseAsync();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task ToggleResetAsync(bool reset)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            switch (_pin)
            {
                case SerialPinType.Rts:
                    _serial.RtsEnable = reset ^ _invert;
                    break;
                case SerialPinType.Dtr:
                    _serial.DtrEnable = reset ^ _invert;
                    break;
                case SerialPinType.Txd:
                    throw new NotImplementedException();
                default:
                    throw new NotSupportedException();
            }
        }

        public void Dispose()
        {
        }
    }
}
