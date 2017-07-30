﻿namespace Brite.Micro.Protocols.STK500v2.Messages
{
    internal class GetSyncRequest : Request
    {
        internal GetSyncRequest()
        {
            Bytes = new[]
            {
                Constants.CmdSignOn
            };
        }
    }
}
