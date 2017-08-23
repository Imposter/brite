using System;
using System.Net;
using System.Threading.Tasks;

namespace Brite.Utility.Network
{
    public interface ITcpServer
    {
        IPEndPoint ListenEndPoint { get; }

        Task StartAsync();
        Task StopAsync();

        event EventHandler<TcpConnectionEventArgs> OnClientConnected;
        event EventHandler<TcpConnectionEventArgs> OnClientDisconnected;
        event EventHandler<TcpReceivedEventArgs> OnDataReceived;
    }
}
