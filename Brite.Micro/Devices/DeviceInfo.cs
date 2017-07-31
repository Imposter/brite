using System.Collections.Generic;
using Brite.Micro.STKv1;

namespace Brite.Micro.Devices
{
    public class DeviceInfo
    {
        private DeviceFlashParameters _flash = new DeviceFlashParameters();
        private DeviceEepromParameters _eeprom = new DeviceEepromParameters();
        private DeviceBits _lockBits = new DeviceBits();
        private DeviceBits _fuseBits = new DeviceBits();
        private readonly IList<MemoryMask> _memoryMasks = new List<MemoryMask>();

        public DeviceFlashParameters Flash => _flash;
        public DeviceEepromParameters Eeprom => _eeprom;
        public DeviceBits LockBits => _lockBits;
        public DeviceBits FuseBits => _fuseBits;
        public IList<MemoryMask> Masks => _memoryMasks;

        public int RamSize { get; set; }
        public Signature Signature { get; set; }
        public StkDeviceCode StkCode { get; set; }

        public bool Verify(MemoryType memType, int address, byte value1, byte value2)
        {
            foreach (var mask in Masks)
            {
                var ch1 = mask.Process(address, memType, ref value1);
                var ch2 = mask.Process(address, memType, ref value2);
                if (ch1 || ch2)
                {
                    return value1 == value2;
                }
            }
            return value1 == value2;
        }
    }
}
