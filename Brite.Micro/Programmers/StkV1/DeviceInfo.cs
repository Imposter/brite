namespace Brite.Micro.Programmers.StkV1
{
    public class DeviceInfo
    {
        private DeviceMemoryInfo _ram;
        private DeviceMemoryInfo _flash;
        private DeviceMemoryInfo _eeprom;
        private DeviceBits _lockBits;
        private DeviceBits _fuseBits;

        private DeviceType _type;
        private int _signature;

        public DeviceType Type => _type;
        public int Signature => _signature;

        public DeviceMemoryInfo Ram => _ram;
        public DeviceMemoryInfo Flash => _flash;
        public DeviceMemoryInfo Eeprom => _eeprom;
        public DeviceBits LockBits => _lockBits;
        public DeviceBits FuseBits => _fuseBits;

        public DeviceInfo(DeviceType type, int signature)
        {
            _type = type;
            _signature = signature;

            _ram = new DeviceMemoryInfo(MemoryType.Ram);
            _flash = new DeviceMemoryInfo(MemoryType.Flash);
            _eeprom = new DeviceMemoryInfo(MemoryType.Eeprom);
            _lockBits = new DeviceBits(MemoryType.LockBits);
            _fuseBits = new DeviceBits(MemoryType.FuseBits);
        }
    }
}
