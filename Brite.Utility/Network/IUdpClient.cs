using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Brite.Utility.Network
{
    public interface IUdpClient
    {
        ushort ListenPort { get; }

        Task BindAsync();
        Task SendRequestAsync(IUdpRequest request);

        // ReSharper disable once ConsiderUsingAsyncSuffix
        Task OnRequestReceived(IUdpRequest request);
    }
}
