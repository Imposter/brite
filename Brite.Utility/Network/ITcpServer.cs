using System;
using System.Net;
using System.Threading.Tasks;

namespace Brite.Utility.Network
{
    public interface ITcpServer<TClient>
    {
        IPEndPoint ListenEndPoint { get; set; }

        Task StartAsync();
        Task StopAsync();

        Task DisconnectAsync(TClient client);
        Task SendAsync(TClient client, byte[] buffer);

        event EventHandler<TcpConnectionEventArgs<TClient>> OnClientConnected;
        event EventHandler<TcpConnectionEventArgs<TClient>> OnClientDisconnected;
        event EventHandler<TcpReceivedEventArgs<TClient>> OnDataReceived;
    }
}
