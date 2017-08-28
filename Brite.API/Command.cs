namespace Brite.API
{
    internal enum Command : byte
    {
        SetId,
        RequestChannel,
        ReleaseChannel,

        DeviceGetVersion,
        DeviceGetId,
        DeviceGetParameters,
        DeviceGetAnimations,
        DeviceSynchronize,

        DeviceSetChannelBrightness,
        DeviceSetChannelLedCount,
        DeviceSetChannelAnimation,
        DeviceSetChannelAnimationEnabled,
        DeviceSetChannelAnimationSpeed,
        DeviceSetChannelAnimationColorCount,
        DeviceSetChannelAnimationColor,
        DeviceSendChannelAnimationRequest, // cast current animation as type and do shizzle (inside server), create instance of each animation handler on startup

        Max
    }
}
