using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Brite.Utility.IO;
using Brite.UWP.Core;
using Windows.Storage.Streams;

namespace Brite.UWP.App
{
    public static class Entrypoint
    {
        public static async Task TMain()
        {
#if TRUE
            var searcher = new SerialDeviceSearcher();
            var devices = await Device.GetDevices<SerialConnection>(searcher);

            foreach (var device in devices)
            {
                try
                {
                    await device.Open(115200, 5000, 10);

                    Log.Info($"Device ID: {device.DeviceId}");
                }
                catch (Exception ex)
                {
                    Log.Error($"Connection Error: {ex}");
                }
            }
#endif

#if FALSE
            var searcher = new SerialDeviceSearcher();
            var devices = await searcher.GetDevices();

            var message = "Hello!";

            foreach (var device in devices)
            {
                try
                {
                    var serial = new SerialConnection(device.PortId, 115200);
                    await serial.Open();

                    await serial.BaseStream.WriteAsync(Encoding.ASCII.GetBytes(message), 0, message.Length);

                    byte[] buffer = new byte[message.Length + 24];
                    var readBytes = await serial.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                    
                    Log.Info(Encoding.ASCII.GetString(buffer, 0, readBytes));
                }
                catch (Exception ex)
                {
                    Log.Error($"Error: {ex}");
                }
            }
#endif
        }
    }
}