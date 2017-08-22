using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Brite.Utility.Network;
using SocketTcpClient = System.Net.Sockets.TcpClient;

namespace Brite.Win.Core.Network
{
    // TODO/NOTE: Implement GetStream in client/server instead and use that with IStream?
    // SocketStream or so...
    public class TcpServer : ITcpServer<SocketTcpClient>
    {
        private class ClientReadArgs
        {
            public SocketTcpClient Client { get; }
            public byte[] Buffer { get; }

            public ClientReadArgs(SocketTcpClient client, byte[] buffer)
            {
                Client = client;
                Buffer = buffer;
            }
        }

        public const int DefaultBufferSize = 2048;

        private TcpListener _server;

        public IPEndPoint ListenEndPoint { get; set; }
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task DisconnectAsync(SocketTcpClient client)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            client.Close();
        }

        public async Task SendAsync(SocketTcpClient client, byte[] buffer)
        {
            var stream = client.GetStream();

            await stream.WriteAsync(buffer, 0, buffer.Length);
            await stream.FlushAsync();
        }

        private void ServerOnTcpClient(IAsyncResult result)
        {
            try
            {
                var client = _server.EndAcceptTcpClient(result);
                OnClientConnected?.Invoke(this, new TcpConnectionEventArgs<SocketTcpClient>(client, (IPEndPoint)client.Client.RemoteEndPoint));
                
                // Create buffer
                var buffer = new byte[BufferSize];

                // Read
                var stream = client.GetStream();
                stream.BeginRead(buffer, 0, buffer.Length, ClientOnReadCallback, new ClientReadArgs(client, buffer));

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
            var buffer = args.Buffer;

            try
            {
                var stream = client.GetStream();
                var bytesRead = stream.EndRead(result);
                if (bytesRead == 0)
                    throw new ObjectDisposedException(nameof(client));

                OnDataReceived?.Invoke(this, new TcpReceivedEventArgs<SocketTcpClient>(client, (IPEndPoint)client.Client.RemoteEndPoint, buffer, bytesRead));

                stream.BeginRead(buffer, 0, buffer.Length, ClientOnReadCallback, new KeyValuePair<SocketTcpClient, byte[]>(client, buffer));
            }
            catch (ObjectDisposedException)
            {
                OnClientDisconnected?.Invoke(this, new TcpConnectionEventArgs<SocketTcpClient>(client, (IPEndPoint)client.Client.RemoteEndPoint));
            }
        }

        public event EventHandler<TcpConnectionEventArgs<SocketTcpClient>> OnClientConnected;
        public event EventHandler<TcpConnectionEventArgs<SocketTcpClient>> OnClientDisconnected;
        public event EventHandler<TcpReceivedEventArgs<SocketTcpClient>> OnDataReceived;
    }
}
