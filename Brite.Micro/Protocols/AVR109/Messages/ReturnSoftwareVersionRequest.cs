namespace Brite.Micro.Protocols.AVR109.Messages
{
    internal class ReturnSoftwareVersionRequest : Request
    {
        internal ReturnSoftwareVersionRequest()
        {
            Bytes = new[]
            {
                Constants.CmdReturnSoftwareVersion
            };
        }
    }
}
