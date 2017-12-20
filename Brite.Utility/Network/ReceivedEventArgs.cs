/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;

namespace Brite.Utility.Network
{
    public class ReceivedEventArgs : EventArgs
    {
        public byte[] Buffer { get; }
        public int Length { get; }

        public ReceivedEventArgs(byte[] buffer, int length)
        {
            Buffer = buffer;
            Length = length;
        }
    }
}
