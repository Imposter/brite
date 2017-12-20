/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

namespace Brite.API
{
    public enum Result : byte
    {
        Ok,
        Error,

        InvalidId,
        NotIdentified,
        IdInUse,

        InvalidDeviceId,
        InvalidChannelIndex,
        InvalidPriority,
        InvalidAnimationId,
        
        DeviceNotOpen,
        AccessDenied,

        NoSupportedAnimations,
        AnimationNotSet
    }
}
