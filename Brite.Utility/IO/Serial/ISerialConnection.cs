/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Threading.Tasks;

namespace Brite.Utility.IO.Serial
{
    public interface ISerialConnection : IDisposable
    {
        string PortName { get; set; }
        uint BaudRate { get; set; }
        int Timeout { get; set; }
        bool DtrEnable { get; set; }
        bool RtsEnable { get; set; }
        bool CtsHolding { get; }
        bool CdHolding { get; }
        bool DsrHolding { get; }
        ushort DataBits { get; set; }
        SerialStopBits StopBits { get; set; }
        SerialParity Parity { get; set; }
        SerialHandshake Handshake { get; set; }
        IStream BaseStream { get; }
        bool IsOpen { get; }

        Task OpenAsync();
        Task CloseAsync();
    }
}
