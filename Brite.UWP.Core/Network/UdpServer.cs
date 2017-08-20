using System;
using System.Net;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Brite.Utility.IO;
using Brite.Utility.Network;

namespace Brite.UWP.Core.Network
{
    public class UdpServer : IUdpServer
    {
        private DatagramSocket _socket;

        public IPAddress Address { get;  }
        public ushort Port { get;  }

        public UdpServer(IPAddress address, ushort port)
        {
            Address = address;
            Port = port;
        }

        public async Task StartAsync()
        {
            if (_socket != null) 
                throw new InvalidOperationException("Server already started");

            _socket = new DatagramSocket();
            _socket.MessageReceived += SocketOnMessageReceived;

            if (!Address.Equals(IPAddress.Any))
                await _socket.BindEndpointAsync(new HostName(Address.ToString()), Port.ToString());
            else await _socket.BindServiceNameAsync(Port.ToString());
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task StopAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_socket == null)
                throw new InvalidOperationException("Server already stopped");

            _socket.Dispose();
            _socket = null;
        }

        public Task OnRequestReceived(IUdpRequest request)
        {
            throw new NotImplementedException();
        }

        private void SocketOnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            Logger.GetLog().DebugAsync($"{sender.Information.RemoteAddress}:{sender.Information.RemotePort}").Wait();
        }
    }
}
