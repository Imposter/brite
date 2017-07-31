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
using Brite.Micro;
using System.IO;
using Windows.Storage.Pickers;
using Windows.Storage;
using Brite.Micro.IO;
using Brite.Micro.STKv1;
using Brite.Micro.Devices;
using DeviceInfo = Brite.Micro.Devices.DeviceInfo;
using Brite.Micro.Hex;

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
                var fileProperties = await file.GetBasicPropertiesAsync();

                using (var reader = new StreamReader(await file.OpenStreamForReadAsync()))
                {
                    var hexFile = HexFile.Parse(reader);

                    using (var serial = new SerialConnection("COM4", 57600))
                    {
                        var channel = new SerialChannel(serial, new ComPin(serial, ComPinType.Dtr, false));

                        var info = new DeviceInfo();
                        info.StkCode = StkDeviceCode.Atmega328;
                        info.Signature = Signature.Parse((int)info.StkCode);
                        info.RamSize = 2048;
                        info.Eeprom.Size = 1024;
                        info.Flash.Size = 32768;
                        info.Flash.PageSize = 128;

                        var client = new StkV1Client(channel);
                        var programmer = new StkV1Programmer(client, info, true);

                        // Start programming
                        var session = await programmer.Start();

                        // TODO: Write nicer interface for this

                        // Write pages
                        // TODO: Write algo for this

                        //int address = 0;
                        //byte[] block = null;
                        //for (int line = 0, lineOffset = 0, blockLength = 0; line < hexFile.Lines.Count;)
                        //{
                        //    if (block == null)
                        //    {
                        //        blockLength = Math.Min(info.Flash.PageSize, hexFile.CodeSize - address);
                        //        block = new byte[hexFile.CodeSize - address];
                        //    }

                        //

                        //    if (address == block.Length)
                        //    {
                        //        await programmer.WritePage(address, AvrMemoryType.Flash, block, 0, block.Length);
                        //        block = null;
                        //    }
                        //}

                        //int address = 0;
                        //int line = 0;
                        //int lineOffset = 0;
                        //do
                        //{
                        //    int blockSize = Math.Min(info.Flash.PageSize, hexFile.CodeSize - address);
                        //    var block = new byte[blockSize];

                        //    //for (int i = 0; i < page.Length; i++)
                        //    //{
                        //    //    var lineData = hexFile.Lines[line].Data;
                        //    //    Array.Copy(lineData, 0, page, i, lineData.Length);
                        //    //}

                        //    //await programmer.WritePage(address, AvrMemoryType.Flash, buffer, address, blockLength);
                        //    address += blockSize;
                        //} while (address < hexFile.CodeSize);

                        // Close programmer
                        await programmer.Stop();
                    }
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