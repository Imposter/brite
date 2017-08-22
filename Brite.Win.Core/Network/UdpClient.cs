using Brite.Utility.Network;
using System;
using System.IO;
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

        public async Task<byte[]> ReceiveAsync()
        {
            var result = await _client.ReceiveAsync();
            if (!result.RemoteEndPoint.Equals(RemoteEndPoint))
                throw new IOException("Received data from unknown host");

            return result.Buffer;
        }

        public async Task SendAsync(byte[] buffer)
        {
            await _client.SendAsync(buffer, buffer.Length, RemoteEndPoint);
        }
    }
}
