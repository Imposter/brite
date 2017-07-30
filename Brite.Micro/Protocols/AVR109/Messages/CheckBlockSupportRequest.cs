namespace Brite.Micro.Protocols.AVR109.Messages
{
    internal class CheckBlockSupportRequest : Request
    {
        internal CheckBlockSupportRequest()
        {
            Bytes = new[]
            {
                Constants.CmdCheckBlockSupport
            };
        }
    }
}
