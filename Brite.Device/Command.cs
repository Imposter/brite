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

        SetChannelBrightness,
        SetChannelLedCount,
        SetChannelAnimation,
        SetChannelAnimationEnabled,
        SetChannelAnimationSpeed,
        SetChannelAnimationColorCount,
        SetChannelAnimationColor,
        SendChannelAnimationRequest,

        BluetoothGetPassword = 50,
        BluetoothSetPassword,
        BluetoothUnpairAll,
        BluetoothGetStatus,
        BluetoothGetConnectionInfo,
    }
}
