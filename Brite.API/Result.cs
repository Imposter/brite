namespace Brite.API
{
    public enum Result : byte
    {
        Ok,
        Error,

        NotIdentified,
        IdInUse,

        InvalidDeviceId,
        InvalidChannelIndex,
        InvalidPriority,
        InvalidAnimationId,

        AccessDenied,

        NoSupportedAnimations,
        AnimationNotSet
    }
}
