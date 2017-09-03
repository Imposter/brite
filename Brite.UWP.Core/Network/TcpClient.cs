using Brite.Utility.IO;
using Brite.Utility.Network;
using Brite.UWP.Core.IO;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Brite.Utility;

namespace Brite.UWP.Core.Network
{
    public class TcpClient : ITcpClient
    {
        public const int DefaultTimeout = 100; // ms

        private StreamSocket _socket;
        private TimedStream _stream;
        private int _timeout;

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
        public bool Connected => _socket != null; // TODO: Fix/improve this

        internal TcpClient(StreamSocket socket, IPEndPoint remoteEndPoint, int timeout = DefaultTimeout)
        {
            _socket = socket;
            RemoteEndPoint = remoteEndPoint;
            _timeout = timeout;
            _stream = new TimedStream(_socket.InputStream, _socket.OutputStream, _timeout);
        }

        public TcpClient(IPEndPoint remoteEndPoint, int timeout = DefaultTimeout)
        {
            RemoteEndPoint = remoteEndPoint;
            _timeout = timeout;
        }

        public async Task ConnectAsync()
        {
            if (_socket != null)
                throw new InvalidOperationException("Client is already connected!");

            _socket = new StreamSocket();
            
            try
            {
                await _socket.ConnectAsync(new HostName(RemoteEndPoint.Address.ToString()), RemoteEndPoint.Port.ToString()).AsTask()
                    .WithCancellation(new CancellationTokenSource(_timeout).Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Unable to connect to the specified host");
            }

            _stream = new TimedStream(_socket.InputStream, _socket.OutputStream, _timeout);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task DisconnectAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_socket == null)
                throw new InvalidOperationException("Client is not connected!");

            _stream.Dispose();
            _stream = null;
            _socket.Dispose();
            _socket = null;
        }

        public IStream GetStream()
        {
            return _stream;
        }
    }
}
