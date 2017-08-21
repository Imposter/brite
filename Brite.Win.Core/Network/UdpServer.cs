using Brite.Utility.Network;
using System;
using System.Net;
using System.Threading.Tasks;
using SocketUdpClient = System.Net.Sockets.UdpClient;

namespace Brite.Win.Core.Network
{
    public class UdpServer : IUdpServer
    {
        private SocketUdpClient _server;

        public IPEndPoint ListenEndPoint { get; set; }

        public UdpServer(IPEndPoint endPoint)
        {
            ListenEndPoint = endPoint;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StartAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_server != null) 
                throw new InvalidOperationException("Already started");

            _server = new SocketUdpClient(ListenEndPoint);
            _server.BeginReceive(ClientOnDataReceived, _server);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_server == null)
                throw new InvalidOperationException("Already stopped");

            _server.Close();
            _server = null;
        }

        public async Task SendResponseAsync(IPEndPoint target, byte[] buffer)
        {
            await _server.SendAsync(buffer, buffer.Length, target);
        }

        private void ClientOnDataReceived(IAsyncResult result)
        {
            try
            {
                IPEndPoint source = new IPEndPoint(IPAddress.Any, 0);
                var buffer = _server.EndReceive(result, ref source);
                OnRequestReceived?.Invoke(this, new UdpReceivedEventArgs(source, buffer));

                _server.BeginReceive(ClientOnDataReceived, _server);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public event EventHandler<UdpReceivedEventArgs> OnRequestReceived;
    }
}
