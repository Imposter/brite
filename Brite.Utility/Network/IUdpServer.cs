using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Brite.Utility.Network
{
    public interface IUdpServer
    {
        IPAddress Address { get;}
        ushort Port { get; }

        Task StartAsync();
        Task StopAsync();
        
        // ReSharper disable once ConsiderUsingAsyncSuffix
        Task OnRequestReceived(IUdpRequest request);
    }
}
