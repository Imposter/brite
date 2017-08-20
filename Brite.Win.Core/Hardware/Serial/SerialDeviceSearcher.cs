using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using Brite.Utility.Hardware.Serial;

namespace Brite.Win.Core.Hardware.Serial
{
    public class SerialDeviceSearcher : ISerialDeviceSearcher
    {
        public async Task<List<SerialDeviceInfo>> GetDevicesAsync()
        {
            return await Task.Run(() =>
            {
                var result = new List<SerialDeviceInfo>();

                var searcher = new ManagementObjectSearcher("SELECT * from Win32_PnPEntity");
                foreach (var obj in searcher.Get())
                {
                    var port = (ManagementObject)obj;

                    var deviceName = (string)port.GetPropertyValue("Caption");
                    var devicePnpid = (string)port.GetPropertyValue("PNPDeviceID");

                    if (deviceName == null || !deviceName.Contains("COM"))
                        continue;

                    // Get device ID
                    var deviceId = deviceName.Substring(deviceName.IndexOf("COM", StringComparison.Ordinal)).TrimEnd(')');

                    // Parse Plug and Play ID
                    var pnpIdSplit = devicePnpid.Split('\\');

                    // Ensure it is a USB device
                    if (pnpIdSplit[0] == "USB")
                    {
                        // Parse vendor id and product id
                        var idSplit = pnpIdSplit[1].Split('&');
                        var vendor = idSplit[0].Split('_')[1];
                        var product = idSplit[1].Split('_')[1];

                        result.Add(new SerialDeviceInfo(deviceName, string.Empty, deviceId, devicePnpid, vendor, product));
                    }
                }

                return result;
            });
        }
    }
}