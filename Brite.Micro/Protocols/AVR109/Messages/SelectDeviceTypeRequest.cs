namespace Brite.Micro.Protocols.AVR109.Messages
{
    internal class SelectDeviceTypeRequest : Request
    {
        internal SelectDeviceTypeRequest(byte deviceCode)
        {
            Bytes = new[]
            {
                Constants.CmdSelectDeviceType,
                deviceCode
            };
        }
    }
}
