using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Brite.Utility.Network;

namespace Brite.UWP.Core.Network
{
   public class UdpClient : IUdpClient
    {
        public ushort ListenPort { get; }

        private readonly DatagramSocket _socket;

        public UdpClient(ushort listenPort)
        {
            ListenPort = listenPort;

            // Create socket
            _socket = new DatagramSocket();
            _socket.MessageReceived += SocketOnMessageReceived;
        }

        public async Task BindAsync()
        {
            await _socket.BindServiceNameAsync(ListenPort.ToString());
        }

        public Task SendRequestAsync(IUdpRequest request)
        {
            // Send request
            //request.
            return null; // TODO: ...
        }

        public Task OnRequestReceived(IUdpRequest request)
        {
            throw new NotImplementedException();
        }

        private void SocketOnMessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
