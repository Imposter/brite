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
using System.IO;
using Windows.Storage.Pickers;
using Windows.Storage;
using Brite.Micro.Programmers;
using Brite.Micro.Programmers.StkV1;
using DeviceInfo = Brite.Micro.Programmers.StkV1.DeviceInfo;

namespace Brite.UWP.App
{
    public static class Entrypoint
    {
        private static Log log = Logger.GetLog();

        public static async Task TMain()
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("Intel HEX File", new List<string> { ".hex" });
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using (var reader = new StreamReader(await file.OpenStreamForWriteAsync()))
                {
                    using (var serial = new SerialConnection("COM3", 115200))
                    {
                        using (var channel = new StkV1Channel(serial))
                        {
                            var info = new DeviceInfo(DeviceType.ATmega328);
                            info.Ram.Size = 2048;
                            info.Flash.Size = 32768;
                            info.Flash.PageSize = 128;
                            info.Eeprom.Size = 1024;
                            info.Eeprom.PageSize = 1;
                            info.LockBits.Size = 6;
                            info.FuseBits.Size = 5;

                            using (var programmer = new StkV1Programmer(channel, info))
                            {
                                await programmer.Open();
                            }
                        }
                    }
                }
            }

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