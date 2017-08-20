using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Brite.Utility.IO;

namespace Brite.Utility.Network
{
    public interface IUdpRequest : IDisposable
    {
        IPEndPoint RemoteEndPoint { get; set; }
        IStream Stream { get; set; }

        Task SendResponseAsync(byte[] buffer);
    }
}
