using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Brite.Utility.IO;
using Brite.Utility.Network;

namespace Brite.UWP.Core.Network
{
    // one for client and one for server since each is different
    [Obsolete]
    public class UdpRequest : IUdpRequest
    {
        public IPEndPoint RemoteEndPoint { get; set; }
        public IStream Stream { get; set; }

        public UdpRequest(IPEndPoint remoteEndPoint)
        {
            Stream = new MemoryStream();
        }

        public UdpRequest(IPEndPoint remoteEndPoint, IStream stream)
        {
            RemoteEndPoint = remoteEndPoint;
            Stream = stream;
        }

        public Task SendResponseAsync(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
