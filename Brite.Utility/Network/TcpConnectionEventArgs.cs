/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Net;

namespace Brite.Utility.Network
{
    public class TcpConnectionEventArgs : EventArgs
    {
        public ITcpClient Client { get; }
        public IPEndPoint Source { get; }

        public TcpConnectionEventArgs(ITcpClient client, IPEndPoint source)
        {
            Client = client;
            Source = source;
        }
    }
}
