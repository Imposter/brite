using Brite.Utility.IO;
using Brite.Utility.Network;
using Brite.Win.Core.IO;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Brite.Utility;
using SocketTcpClient = System.Net.Sockets.TcpClient;

namespace Brite.Win.Core.Network
{
    public class TcpClient : ITcpClient
    {
        public const int DefaultTimeout = 100; // ms

        private int _timeout;
        private SocketTcpClient _client;
        private TimedStream _stream;

        public int Timeout
        {
            get => _timeout;
            set
            {
                _timeout = value;
                if (_stream != null)
                    _stream.Timeout = value;
            }
        }

        public IPEndPoint RemoteEndPoint { get; }
        public bool Connected => _client != null && _client.Connected;

        internal TcpClient(SocketTcpClient client, IPEndPoint remoteEndPoint, int timeout = DefaultTimeout)
        {
            _client = client;
            RemoteEndPoint = remoteEndPoint;
            _timeout = timeout;
            _stream = new TimedStream(client.GetStream());
        }

        public TcpClient(IPEndPoint remoteEndPoint, int timeout = DefaultTimeout)
        {
            RemoteEndPoint = remoteEndPoint;
            _timeout = timeout;
        }

        public async Task ConnectAsync()
        {
            if (_client != null)
                throw new InvalidOperationException("Client is already connected!");

            _client = new SocketTcpClient();

            try
            {
                await _client.ConnectAsync(RemoteEndPoint.Address, RemoteEndPoint.Port)
                    .WithCancellation(new CancellationTokenSource(_timeout).Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Unable to connect to the specified host");
            }

            _stream = new TimedStream(_client.GetStream(), _timeout);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task DisconnectAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_client == null)
                throw new InvalidOperationException("Client is not connected!");

            _stream.Dispose();
            _stream = null;
            _client.Dispose();
            _client = null;
        }

        public IStream GetStream()
        {
            return _stream;
        }
    }
}
