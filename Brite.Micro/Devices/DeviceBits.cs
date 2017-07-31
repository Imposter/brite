using System.Collections.Generic;
using System.Linq;

namespace Brite.Micro.Devices
{
    public class DeviceBits
    {
        private readonly IList<DeviceBitsGroup> _groups = new List<DeviceBitsGroup>();
        public IList<DeviceBitsGroup> Groups => _groups;

        public int StartAddress
        {
            get
            {
                return _groups.Count == 0 ? 0 : _groups.Min(item => item.StartAddress);
            }
        }

        public int EndAddress
        {
            get
            {
                return _groups.Count == 0 ? 0 : _groups.Max(item => item.EndAddress);
            }
        }

        public int Size => EndAddress - StartAddress + 1;
        public MemoryType? Location { get; set; }
        public int PageSize { get; set; }
    }
}
