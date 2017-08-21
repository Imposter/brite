using System;
using System.Net;

namespace Brite.Utility.Network
{
    public class UdpReceivedEventArgs : EventArgs
    {
        public IPEndPoint Source { get; }
        public byte[] Buffer { get; }

        public UdpReceivedEventArgs(IPEndPoint source, byte[] buffer)
        {
            Source = source;
            Buffer = buffer;
        }
    }
}
