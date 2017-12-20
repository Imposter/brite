/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

namespace Brite.API
{
    internal enum Command : byte
    {
        SetId,
        GetDevices,

        // Only for opening/closing the device
        RequestDevice,
        ReleaseDevice,
        OpenDevice,
        CloseDevice,

        RequestDeviceChannel,
        ReleaseDeviceChannel,

        DeviceGetVersion,
        DeviceGetParameters,
        DeviceGetAnimations,
        DeviceSynchronize,
        DeviceChannelReset,
        DeviceSetChannelBrightness,
        DeviceSetChannelLedCount,
        DeviceSetChannelAnimation,
        DeviceSetChannelAnimationEnabled,
        DeviceSetChannelAnimationSpeed,
        DeviceSetChannelAnimationColorCount,
        DeviceSetChannelAnimationColor,
        DeviceSendChannelAnimationRequest,

        Max
    }
}
