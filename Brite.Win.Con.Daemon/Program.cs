using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brite.API;
using Brite.API.Animations.Server;
using Brite.Utility.IO;
using Brite.Win.Core.Hardware.Serial;
using Brite.Win.Core.IO.Serial;
using Brite.Win.Core.Network;

namespace Brite.Win.Con.Daemon
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateAndRunServerAsync().Wait();

            Thread.Sleep(-1);
        }

        static BriteServer server;
        static async Task CreateAndRunServerAsync()
        {
            var listenEndPoint = new IPEndPoint(IPAddress.Any, 2200);
            var serverLayer = new TcpServer(listenEndPoint);

            var deviceSearcher = new SerialDeviceSearcher();
            var devices = await Device.GetDevicesAsync<SerialConnection>(deviceSearcher);

            Console.WriteLine("Available devices: ");
            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                Console.WriteLine($"{i}) {device.Info.PortName}");
            }

            Console.WriteLine();
            Console.Write("Select device: ");
            string port = Console.ReadLine();

            Device briteDevice = null;
            foreach (var device in devices)
            {
                if (device.Info.PortName == port)
                {
                    briteDevice = device;
                    break;
                }
            }

            if (briteDevice == null)
            {
                Console.WriteLine("Invalid device");
                await Task.Delay(1000);
                Environment.Exit(-1);
            }

            server = new BriteServer(serverLayer);
            server.AddAnimation(new ManualAnimation());
            server.AddDevice(briteDevice);

            await briteDevice.OpenAsync(115200, 1000, 10);

            await server.StartAsync(); // NOTE: While updating, the server must be off
        }
    }
}
