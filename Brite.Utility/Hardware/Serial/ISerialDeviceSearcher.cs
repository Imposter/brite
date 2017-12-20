/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brite.Utility.Hardware.Serial
{
    public interface ISerialDeviceSearcher
    {
        Task<List<SerialDeviceInfo>> GetDevicesAsync();
    }
}
