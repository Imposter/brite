namespace Brite.API
{
    internal enum Result : byte
    {
        Ok,
        Error,

        NotIdentified,

        InvalidDeviceId,
        InvalidChannelIndex,
        InvalidPriority,
        InvalidAnimationId,

        AccessDenied,

        NoSupportedAnimations,
        AnimationNotSet
    }
}
