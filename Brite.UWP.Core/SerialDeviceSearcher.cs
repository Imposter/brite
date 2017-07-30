using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Brite.Utility.IO;

namespace Brite.UWP.Core
{
    public class SerialDeviceSearcher : IDeviceSearcher
    {
        public async Task<List<DeviceInfo>> GetDevices()
        {
            var result = new List<DeviceInfo>();

            var selector = SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);

            foreach (var device in devices)
            {
                // Get portName
                var portName = device.Name.Substring(device.Name.IndexOf("COM", StringComparison.Ordinal)).TrimEnd(')');

                // Parse Plug and Play ID
                var pnpId = device.Id.Substring(device.Id.LastIndexOf('\\') + 1);
                var pnpIdSplit = pnpId.Split('#');

                // Ensure it is a USB device (TODO: Add support for Bluetooth)
                if (pnpIdSplit[0] == "USB")
                {
                    // Parse vendor id and product id
                    var idSplit = pnpIdSplit[1].Split('&');
                    var vendor = idSplit[0].Split('_')[1];
                    var product = idSplit[1].Split('_')[1];

                    result.Add(new DeviceInfo(device.Name, string.Empty, portName, device.Id, vendor, product));
                }
            }

            return result;
        }
    }
}