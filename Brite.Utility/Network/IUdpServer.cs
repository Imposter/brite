/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Net;
using System.Threading.Tasks;

namespace Brite.Utility.Network
{
    public interface IUdpServer
    {
        IPEndPoint ListenEndPoint { get; }

        Task StartAsync();
        Task StopAsync();

        Task SendAsync(IPEndPoint target, byte[] buffer);

        event EventHandler<UdpReceivedEventArgs> OnDataReceived;
    }
}
