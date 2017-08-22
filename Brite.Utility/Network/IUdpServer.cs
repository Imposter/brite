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
