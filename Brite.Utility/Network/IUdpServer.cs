﻿using System;
using System.Net;
using System.Threading.Tasks;

namespace Brite.Utility.Network
{
    public interface IUdpServer
    {
        IPEndPoint ListenEndPoint { get; set; }

        Task StartAsync();
        Task StopAsync();

        Task SendResponseAsync(IPEndPoint target, byte[] buffer);

        event EventHandler<UdpReceivedEventArgs> OnRequestReceived;
    }
}
