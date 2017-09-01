namespace Brite.API
{
    internal enum Command : byte
    {
        SetId,
        GetDevices,
        RequestDeviceChannel,
        ReleaseDeviceChannel,

        DeviceGetVersion,
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
