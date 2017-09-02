using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Brite.API;
using Brite.API.Animations.Client;
using Brite.Win.Core.Network;

namespace Brite.Win.Con.RocketLeague.Test
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var attribute = (GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0];
            var id = attribute.Value;

            var tcpClient = new TcpClient(new IPEndPoint(IPAddress.Loopback, 6450));
            var client = new BriteClient(tcpClient, $"{{{id}-{new Random().Next(0, 255)}}}");

            Thread.Sleep(5000);

            while (true)
            {
                try
                {
                    client.ConnectAsync().Wait();
                    break;
                }
                catch
                {
                    
                }
            }

            Console.WriteLine("Connected");

            var dev1 = client.Devices[0];
            var channel = dev1.Channels[0];
            channel.RequestAsync().Wait();
            channel.SetSizeAsync(32).Wait(); // NOTE: These aren't correctly implemented
            channel.SetBrightnessAsync(255).Wait();
            var anim = new BreatheAnimation();
            channel.SetAnimationAsync(anim).Wait();
            anim.SetColorCountAsync(1).Wait();
            anim.SetColorAsync(0, new Color(255, 0, 0)).Wait();
            anim.SetSpeedAsync(1.0f).Wait();
            anim.SetEnabledAsync(true).Wait();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}
