﻿using Brite.Micro.Hardware.Memory;

namespace Brite.Micro.Protocols.STK500v2.Messages
{
    internal class ExecuteReadPageRequest : Request
    {
        internal ExecuteReadPageRequest(byte readCmd, IMemory memory)
        {
            var pageSize = memory.PageSize;
            var cmdByte = memory.CmdBytesRead[0];
            Bytes = new[]
            {
                readCmd,
                (byte)(pageSize >> 8),
                (byte)(pageSize & 0xff),
                cmdByte
            };
        }
    }
}
