using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brite.Utility.IO;
using Brite.Win.Core.Network;

namespace Brite.Win.Con.Daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            // Server test
            var server = new TcpServer(new IPEndPoint(IPAddress.Any, 2200));
            server.OnClientConnected += (sender, eventArgs) =>
            {
                Console.WriteLine("Client connected!");
            };
            server.OnClientDisconnected += (sender, eventArgs) =>
            {
                Console.WriteLine("Client disconnected");
            };
            server.OnDataReceived += (sender, eventArgs) =>
            {
                Console.WriteLine("Data received");
                Console.WriteLine(Encoding.ASCII.GetString(eventArgs.Buffer));
            };
            server.StartAsync().Wait();

            Thread.Sleep(-1);
        }
    }
}
