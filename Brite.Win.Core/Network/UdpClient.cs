using Brite.Utility.Network;
using System;
using System.Net;
using System.Threading.Tasks;
using SocketUdpClient = System.Net.Sockets.UdpClient;

namespace Brite.Win.Core.Network
{
    public class UdpClient : IUdpClient
    {
        private SocketUdpClient _client;

        public IPEndPoint ListenEndPoint { get; set; }
        public IPEndPoint RemoteEndPoint { get; set; }

        public UdpClient(IPEndPoint listenEndpoint, IPEndPoint remoteEndPoint)
        {
            ListenEndPoint = listenEndpoint;
            RemoteEndPoint = remoteEndPoint;
        }

        IPEndPoint IUdpClient.ListenEndPoint { get; set; }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task ConnectAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_client != null)
                throw new InvalidOperationException("Client is already connected!");

            _client = new SocketUdpClient(ListenEndPoint);
            _client.BeginReceive(ClientOnDataReceived, _client);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task DisconnectAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_client == null)
                throw new InvalidOperationException("Client is not connected!");

            _client.Close();
            _client = null;
        }

        public async Task SendRequestAsync(byte[] buffer)
        {
            await _client.SendAsync(buffer, buffer.Length, RemoteEndPoint);
        }

        private void ClientOnDataReceived(IAsyncResult result)
        {
            try
            {
                IPEndPoint source = new IPEndPoint(RemoteEndPoint.Address, RemoteEndPoint.Port);
                var buffer = _client.EndReceive(result, ref source);
                OnResponseReceived?.Invoke(this, new UdpReceivedEventArgs(source, buffer));

                _client.BeginReceive(ClientOnDataReceived, _client);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public event EventHandler<UdpReceivedEventArgs> OnResponseReceived;
    }
}
