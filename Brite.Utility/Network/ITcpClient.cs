using System;
using System.Net;
using System.Threading.Tasks;

namespace Brite.Utility.Network
{
    public interface ITcpClient
    {
        int Timeout { get; set; }
        IPEndPoint RemoteEndPoint { get; set; }

        Task ConnectAsync();
        Task DisconnectAsync();

        Task ReceiveAsync(byte[] buffer);
        Task SendAsync(byte[] buffer);
    }
}