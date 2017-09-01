using Brite.Utility.Network;
using System;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Brite.UWP.Core.Network
{
    public class TcpServer : ITcpServer
    {
        public const int DefaultBufferSize = 2048;

        private StreamSocketListener _listener;

        public IPEndPoint ListenEndPoint { get; }
        public bool Running => _listener != null;
        public int BufferSize { get; set; }

        public TcpServer(IPEndPoint listenEndPoint, int bufferSize = DefaultBufferSize)
        {
            ListenEndPoint = listenEndPoint;
            BufferSize = bufferSize;
        }

        public async Task StartAsync()
        {
            if (_listener != null)
                throw new InvalidOperationException("Already started");

            _listener = new StreamSocketListener();
            _listener.Control.KeepAlive = true;
            _listener.ConnectionReceived += ListenerOnConnectionReceived;
            if (ListenEndPoint.Address.Equals(IPAddress.Any))
                await _listener.BindServiceNameAsync(ListenEndPoint.Port.ToString());
            else
                await _listener.BindEndpointAsync(new HostName(ListenEndPoint.Address.ToString()),
                    ListenEndPoint.Port.ToString());
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_listener == null)
                throw new InvalidOperationException("Already stopped");

            _listener.Dispose();
            _listener = null;
        }

        private void ListenerOnConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse(args.Socket.Information.RemoteAddress.RawName), int.Parse(args.Socket.Information.RemotePort));
            var client = new TcpClient(args.Socket, ipEndPoint);

            OnClientConnected?.Invoke(this, new TcpConnectionEventArgs(client, ipEndPoint));

            Task.Run(async () =>
            {
                try
                {
                    var buffer = new byte[BufferSize];
                    while (true)
                    {
                        var result = await args.Socket.InputStream.ReadAsync(buffer.AsBuffer(), (uint)buffer.Length, InputStreamOptions.Partial);
                        if (result.Length > 0)
                        {
                            OnDataReceived?.Invoke(this, new TcpReceivedEventArgs(client, ipEndPoint, buffer, (int)result.Length));
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    OnClientDisconnected?.Invoke(this, new TcpConnectionEventArgs(client, ipEndPoint));
                }
            });
        }

        public event EventHandler<TcpConnectionEventArgs> OnClientConnected;
        public event EventHandler<TcpConnectionEventArgs> OnClientDisconnected;
        public event EventHandler<TcpReceivedEventArgs> OnDataReceived;
    }
}
