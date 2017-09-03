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
