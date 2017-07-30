﻿namespace Brite.Micro.Protocols.AVR109.Messages
{
    internal class ReadSignatureBytesRequest : Request
    {
        internal ReadSignatureBytesRequest()
        {
            Bytes = new[]
            {
                Constants.CmdReadSignatureBytes
            };
        }
    }
}
