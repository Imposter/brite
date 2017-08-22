using System.Net;

namespace Brite.Utility.Network
{
    public class UdpReceivedEventArgs : ReceivedEventArgs
    {
        public IPEndPoint Source { get; }

        public UdpReceivedEventArgs(IPEndPoint source, byte[] buffer, int length)
            : base(buffer, length)
        {
            Source = source;
        }
    }
}
