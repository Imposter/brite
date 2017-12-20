/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

namespace Brite.Utility.Hardware.Serial
{
    public class SerialDeviceInfo : DeviceInfo
    {
        public string PortName { get; }

        public SerialDeviceInfo(string name, string description, string portName, string pnpId, string vendorId, string productId) 
            : base(name, description, pnpId, vendorId, productId)
        {
            PortName = portName;
        }
    }
}
