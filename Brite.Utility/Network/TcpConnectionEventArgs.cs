using System;
using System.Net;

namespace Brite.Utility.Network
{
    public class TcpConnectionEventArgs<TClient> : EventArgs
    {
        public TClient Client { get; }
        public IPEndPoint Source { get; }

        public TcpConnectionEventArgs(TClient client, IPEndPoint source)
        {
            Client = client;
            Source = source;
        }
    }
}
