using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.System;
using Brite.Micro;
using Brite.Micro.Formats;
using Brite.Micro.Programmers;
using Brite.Micro.Programmers.StkV1;
using Brite.Utility.IO;
using Brite.UWP.Core;
using Brite.UWP.Core.IO;
using Brite.UWP.Core.IO.Serial;

namespace Brite.UWP.App
{
    public static class Entrypoint
    {
        private static readonly Log log = Logger.GetLog();

        public static async Task TMainAsync()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".hex");
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Read file
                using (var fileStream = await file.OpenSequentialReadAsync())
                {
                    // Create serializer
                    var serializer = new IntelBinarySerializer();
                    
                    // Deserialize binary
                    using (var memoryStream = await serializer.DeserializeAsync(new Stream(fileStream)))
                    {
                        var buffer = memoryStream.ToArray();

                        // Create serial connection
                        using (var serial = new SerialConnection("COM4", 57600))
                        {
                            using (var channel = new Channel(serial))
                            {
                                var info = new DeviceInfo(DeviceType.ATmega328);
                                info.Ram.Size = 2048;
                                info.Flash.Size = 32768;
                                info.Flash.PageSize = 128;
                                info.Eeprom.Size = 1024;
                                info.Eeprom.PageSize = 1;
                                info.LockBits.Size = 1;
                                info.FuseBits.Size = 3;
                                
                                // Create programmer using device information
                                using (var programmer = new StkV1Programmer(channel, info))
                                {
                                    // Open a connection with the device
                                    await programmer.OpenAsync();

                                    // Write to device
                                    await programmer.WriteAsync(MemoryType.Flash, buffer, 0, buffer.Length);

                                    // Reset device
                                    await programmer.ResetAsync();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}