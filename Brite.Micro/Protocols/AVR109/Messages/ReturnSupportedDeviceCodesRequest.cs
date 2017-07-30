namespace Brite.Micro.Protocols.AVR109.Messages
{
    internal class ReturnSupportedDeviceCodesRequest : Request
    {
        internal ReturnSupportedDeviceCodesRequest()
        {
            Bytes = new[]
            {
                Constants.CmdReturnSupportedDeviceCodes
            };
        }
    }
}
