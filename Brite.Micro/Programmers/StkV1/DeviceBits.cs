using System.Collections.Generic;

namespace Brite.Micro.Programmers.StkV1
{
    public class DeviceBits
    {
        private DeviceMemoryInfo _info;
        private List<DeviceBit> _bits;

        public DeviceBits(MemoryType memory)
        {
            _info = new DeviceMemoryInfo(memory);
            _bits = new List<DeviceBit>();
        }
    }
}
