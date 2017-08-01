using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Storage.Streams;
using System.IO;
using Windows.Storage.Pickers;
using Windows.Storage;
using Brite.Micro.Programmers;
using Brite.Micro.Programmers.StkV1;
using Brite.Utility.IO;
using Brite.UWP.Core;

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
                        using (var channel = new Channel(serial))
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

                                log.Info("Connection established");

                                // TODO: Dump to hex file
                                // http://www.interlog.com/~speff/usefulinfo/Hexfrmt.pdf
                            }
                        }
                    }
                }
            }
        }
    }
}