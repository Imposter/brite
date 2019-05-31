using Brite.API.Animations.Client;
using Brite.API.Client;
using Brite.Win.Core.Network;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Brite.Win.Con.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                // Connect to server
                var client = new BriteClient(new TcpClient(new IPEndPoint(IPAddress.Loopback, 6450)), "TestClient");
                try
                {
                    await client.ConnectAsync();

                    foreach (var device in client.Devices)
                    {
                        foreach (var channel in device.Channels)
                        {
                            // Request channel
                            await channel.RequestAsync();

                            // Perform changes
                            var animation = new BreatheAnimation();

                            await channel.SetSizeAsync(12);
                            await channel.SetBrightnessAsync(255);
                            await channel.SetAnimationAsync(animation);
                            await animation.SetSpeedAsync(0.925f);
                            await animation.SetEnabledAsync(true);

                            // Release channel
                            await channel.ReleaseAsync();
                        }

                        var random = new Random();
                        while (true)
                        {
                            foreach (var channel in device.Channels)
                            {
                                // Request channel
                                await channel.RequestAsync();

                                var animation = channel.Animation;
                                await animation.SetColorCountAsync(animation.MaxColors);
                                for (var i = 0; i < animation.MaxColors; i++)
                                    await animation.SetColorAsync((byte)i, new Color(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)));
                                
                                await Task.Delay(100);

                                // Release channel
                                await channel.ReleaseAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to connect to server");
                    Console.WriteLine(ex);
                }
            }).Wait();
        }
    }
}
