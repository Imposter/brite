using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brite.Utility.IO;

namespace Brite.Micro.Programmers
{
    public class StkV1Programmer : SerialProgrammer
    {
        private DeviceInfo _info;

        public StkV1Programmer(SerialChannel channel, DeviceInfo info) 
            : base(channel)
        {
            _info = info;
        }

        public override async Task Open()
        {
            await base.Open();
        }

        public override Task ReadPage(int address, MemoryType type, byte[] data, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public override Task WritePage(int address, MemoryType type, byte[] data, int offset, int length)
        {
            throw new NotImplementedException();
        }

        // TODO: Classes for STK, then, STKClient methods here
    }
}
