using System.Collections.Generic;

namespace Brite.Micro.Devices
{
    public class DeviceByteBits
    {
        public int Address { get; set; }
        public IList<DeviceBit> Bits { get; set; }
    }
}
