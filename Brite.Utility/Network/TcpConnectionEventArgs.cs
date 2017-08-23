using System;
using System.Net;

namespace Brite.Utility.Network
{
    public class TcpConnectionEventArgs : EventArgs
    {
        public ITcpClient Client { get; }
        public IPEndPoint Source { get; }

        public TcpConnectionEventArgs(ITcpClient client, IPEndPoint source)
        {
            Client = client;
            Source = source;
        }
    }
}
