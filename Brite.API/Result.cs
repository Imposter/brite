﻿namespace Brite.API
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
