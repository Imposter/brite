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
using Brite.Micro.Hardware;
using Brite.Micro.BootloaderProgrammers;
using Brite.Micro;
using System.IO;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace Brite.UWP.App
{
    public static class Entrypoint
    {
        private static Log log = Logger.GetLog();

        public static async Task TMain()
        {
#if TRUE
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".hex");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var microcontroller = new AtMega328P();
                var programmer = new OptibootBootloaderProgrammer<SerialConnection>(new SerialConfig("COM4", 57600), microcontroller);

                using (var reader = new StreamReader(await file.OpenStreamForReadAsync()))
                {
                    await DeviceUploader.Upload(programmer, microcontroller, Encoding.ASCII.GetBytes(await reader.ReadToEndAsync()));
                }
            }
#endif

#if FALSE
            var searcher = new SerialDeviceSearcher();
            var devices = await Device.GetDevices<SerialConnection>(searcher);

            foreach (var device in devices)
            {
                try
                {
                    await device.Open(115200, 5000, 10);

                    log.Info($"Device ID: {device.DeviceId}");
                }
                catch (Exception ex)
                {
                    log.Error($"Connection Error: {ex}");
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
                    
                    log.Info(Encoding.ASCII.GetString(buffer, 0, readBytes));
                }
                catch (Exception ex)
                {
                    log.Error($"Error: {ex}");
                }
            }
#endif
        }
    }
}