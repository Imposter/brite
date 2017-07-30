namespace Brite.Micro.Protocols.AVR109.Messages
{
    internal class ExitBootLoaderRequest : Request
    {
        internal ExitBootLoaderRequest()
        {
            Bytes = new[]
            {
                Constants.CmdExitBootloader
            };
        }
    }
}
