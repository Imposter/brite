using System.Net;

namespace Brite.Utility.Network
{
    public class TcpReceivedEventArgs<TClient> : ReceivedEventArgs // TODO/NOTE: Use stream?
    {
        public TClient Client { get; }
        public IPEndPoint Source { get; }

        public TcpReceivedEventArgs(TClient client, IPEndPoint source, byte[] buffer, int length)
            : base(buffer, length)
        {
            Client = Client;
            Source = source;
        }
    }
}
