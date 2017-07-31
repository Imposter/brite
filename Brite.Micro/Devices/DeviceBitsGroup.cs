using System.Collections.Generic;
using System.Linq;

namespace Brite.Micro.Devices
{
    public class DeviceBitsGroup
    {
        private readonly IList<DeviceBit> _bits = new List<DeviceBit>();
        public IList<DeviceBit> Bits => _bits;

        public string Name { get; set; }
        public string Description { get; set; }

        public int StartAddress
        {
            get
            {
                return _bits.Count == 0 ? 0 : _bits.Min(item => item.Address);
            }
        }

        public int EndAddress
        {
            get
            {
                return _bits.Count == 0 ? 0 : _bits.Max(item => item.Address);
            }
        }

        public IList<DeviceBit> VisibleBits
        {
            get { return _bits.Where(item => !item.Hidden).ToList(); }
        }
    }
}
