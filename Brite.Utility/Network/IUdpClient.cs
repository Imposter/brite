﻿using System.Net;
using System.Threading.Tasks;

namespace Brite.Utility.Network
{
    public interface IUdpClient
    {
        IPEndPoint ListenEndPoint { get; set; }
        IPEndPoint RemoteEndPoint { get; set; }

        Task ConnectAsync();
        Task DisconnectAsync();

        Task<byte[]> ReceiveAsync();
        Task SendAsync(byte[] buffer);
    }
}