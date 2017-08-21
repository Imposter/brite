using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brite.Win.Core.Network;

namespace Brite.Win.Con.Daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            // Server/Client test
            var server = new UdpServer(new IPEndPoint(IPAddress.Any, 8000));
            server.OnRequestReceived += async (sender, eventArgs) =>
            {
                Console.WriteLine("C>S [{0}]: {1}", eventArgs.Source, Encoding.ASCII.GetString(eventArgs.Buffer));
                await server.SendResponseAsync(eventArgs.Source, eventArgs.Buffer);
            };

            server.StartAsync().Wait();

            var client = new UdpClient(new IPEndPoint(IPAddress.Any, 50000), new IPEndPoint(IPAddress.Loopback, server.ListenEndPoint.Port));
            client.OnResponseReceived += (sender, eventArgs) =>
            {
                Console.WriteLine("S>C [{0}]: {1}", eventArgs.Source, Encoding.ASCII.GetString(eventArgs.Buffer));
            };

            client.ConnectAsync().Wait();

            while (true)
            {
                client.SendRequestAsync(Encoding.ASCII.GetBytes("Hello")).Wait();
                Thread.Sleep(1000);
            }
        }
    }
}
