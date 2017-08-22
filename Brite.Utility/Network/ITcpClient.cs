using System;
using System.Net;
using System.Threading.Tasks;
using Brite.Utility.IO;

namespace Brite.Utility.Network
{
    public interface ITcpClient
    {
        int Timeout { get; set; }
        IPEndPoint RemoteEndPoint { get; }

        Task ConnectAsync();
        Task DisconnectAsync();

        IStream GetStream();
    }
}