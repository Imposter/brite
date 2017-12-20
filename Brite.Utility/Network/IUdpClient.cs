/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System.Net;
using System.Threading.Tasks;

namespace Brite.Utility.Network
{
    public interface IUdpClient
    {
        IPEndPoint ListenEndPoint { get; }
        IPEndPoint RemoteEndPoint { get; }

        Task ConnectAsync();
        Task DisconnectAsync();

        Task<byte[]> ReceiveAsync();
        Task SendAsync(byte[] buffer);
    }
}
