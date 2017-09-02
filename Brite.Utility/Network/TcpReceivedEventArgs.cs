using System.Net;

namespace Brite.Utility.Network
{
    public class TcpReceivedEventArgs : ReceivedEventArgs
    {
        public ITcpClient Client { get; }
        public IPEndPoint Source { get; }

        public TcpReceivedEventArgs(ITcpClient client, IPEndPoint source, byte[] buffer, int length)
            : base(buffer, length)
        {
            Client = client;
            Source = source;
        }
    }
}
