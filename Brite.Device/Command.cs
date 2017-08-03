namespace Brite.Device
{
    internal enum Command : byte
    {
        GetVersion = 0,
        GetId,
        SetId,
        Reset,
        GetParameters,
        GetAnimations,
        Synchronize,

        SetChannelBrightness,
        SetChannelLedCount,
        SetChannelAnimation,
        SetChannelAnimationEnabled,
        SetChannelAnimationSpeed,
        SetChannelAnimationColorCount,
        SetChannelAnimationColor,
        SendChannelAnimationRequest
    }
}
