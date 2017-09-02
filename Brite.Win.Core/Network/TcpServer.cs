using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Brite.Utility.Network;
using SocketTcpClient = System.Net.Sockets.TcpClient;

namespace Brite.Win.Core.Network
{
    public class TcpServer : ITcpServer
    {
        private class ClientReadArgs
        {
            public TcpClient Client { get; }
            public SocketTcpClient InternalClient { get; }
            public byte[] Buffer { get; }

            public ClientReadArgs(TcpClient client, SocketTcpClient internalClient, byte[] buffer)
            {
                Client = client;
                InternalClient = internalClient;
                Buffer = buffer;
            }
        }

        public const int DefaultBufferSize = 2048;

        private TcpListener _server;

        public IPEndPoint ListenEndPoint { get; }
        public bool Running => _server != null;
        public bool AutoReceive { get; set; }
        public int BufferSize { get; set; }

        public TcpServer(IPEndPoint endPoint, int bufferSize = DefaultBufferSize)
        {
            ListenEndPoint = endPoint;
            BufferSize = bufferSize;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StartAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_server != null)
                throw new InvalidOperationException("Already started");

            _server = new TcpListener(ListenEndPoint);
            _server.Start();

            _server.BeginAcceptTcpClient(ServerOnTcpClient, _server);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_server == null)
                throw new InvalidOperationException("Already stopped");

            _server.Stop();
            _server = null;
        }

        private void ServerOnTcpClient(IAsyncResult result)
        {
            try
            {
                if (_server == null)
                    return;

                var tcpClient = _server.EndAcceptTcpClient(result);
                var client = new TcpClient(tcpClient, (IPEndPoint)tcpClient.Client.RemoteEndPoint);
                OnClientConnected?.Invoke(this, new TcpConnectionEventArgs(client, client.RemoteEndPoint));

                if (AutoReceive)
                {
                    // Create buffer
                    var buffer = new byte[BufferSize];

                    // Read
                    var stream = tcpClient.GetStream();
                    stream.BeginRead(buffer, 0, buffer.Length, ClientOnReadCallback, new ClientReadArgs(client, tcpClient, buffer));
                }

                _server.BeginAcceptTcpClient(ServerOnTcpClient, _server);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void ClientOnReadCallback(IAsyncResult result)
        {
            var args = (ClientReadArgs)result.AsyncState;
            var client = args.Client;
            var tcpClient = args.InternalClient;
            var buffer = args.Buffer;

            try
            {
                var stream = tcpClient.GetStream();
                var bytesRead = stream.EndRead(result);
                if (bytesRead == 0)
                    throw new ObjectDisposedException(nameof(client));

                OnDataReceived?.Invoke(this, new TcpReceivedEventArgs(client, client.RemoteEndPoint, buffer, bytesRead));

                stream.BeginRead(buffer, 0, buffer.Length, ClientOnReadCallback, new ClientReadArgs(client, tcpClient, buffer));
            }
            catch (Exception)
            {
                OnClientDisconnected?.Invoke(this, new TcpConnectionEventArgs(client, client.RemoteEndPoint));
            }
        }

        public event EventHandler<TcpConnectionEventArgs> OnClientConnected;
        public event EventHandler<TcpConnectionEventArgs> OnClientDisconnected;
        public event EventHandler<TcpReceivedEventArgs> OnDataReceived;
    }
}
