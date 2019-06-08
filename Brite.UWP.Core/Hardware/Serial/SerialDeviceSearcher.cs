/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Brite.Utility.Hardware.Serial;

using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

namespace Brite.UWP.Core.Hardware.Serial
{
    public class SerialDeviceSearcher : ISerialDeviceSearcher
    {
        public async Task<List<SerialDeviceInfo>> GetDevicesAsync()
        {
            var result = new List<SerialDeviceInfo>();

            var selector = SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);

            foreach (var device in devices)
            {
                // Get portName
                var portName = device.Name.Substring(device.Name.IndexOf("COM", StringComparison.Ordinal)).TrimEnd(')');

                // Parse Plug and Play ID
                var pnpId = device.Id.Substring(device.Id.LastIndexOf('\\') + 1);
                var pnpIdSplit = pnpId.Split('#');

                // Ensure it is a USB device
                if (pnpIdSplit[0] == "USB")
                {
                    // Parse vendor id and product id
                    var idSplit = pnpIdSplit[1].Split('&');
                    var vendor = idSplit[0].Split('_')[1];
                    var product = idSplit[1].Split('_')[1];

                    result.Add(new SerialDeviceInfo(device.Name, string.Empty, portName, device.Id, vendor, product));
                }
            }

            return result;
        }
    }
}
