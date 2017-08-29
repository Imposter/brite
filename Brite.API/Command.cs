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
        DeviceSendChannelAnimationRequest,

        Max
    }
}
