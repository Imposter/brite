/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System.Net;

namespace Brite.Utility.Network
{
    public class TcpReceivedEventArgs : ReceivedEventArgs
    {
        public ITcpClient Client { get; }
        public IPEndPoint Source { get; }

        public TcpReceivedEventArgs(ITcpClient client, IPEndPoint source, byte[] buffer, int length)
            : base(buffer, length)
        {
            Client = client;
            Source = source;
        }
    }
}
