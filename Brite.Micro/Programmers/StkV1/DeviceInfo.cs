namespace Brite.Micro.Programmers.StkV1
{
    public class DeviceInfo
    {
        public DeviceType Type { get; }
        public DeviceMemoryInfo Ram { get; }
        public DeviceMemoryInfo Flash { get; }
        public DeviceMemoryInfo Eeprom { get; }
        public DeviceMemoryInfo LockBits { get; }
        public DeviceMemoryInfo FuseBits { get; }

        public DeviceInfo(DeviceType type)
        {
            Type = type;

            Ram = new DeviceMemoryInfo(MemoryType.Ram);
            Flash = new DeviceMemoryInfo(MemoryType.Flash);
            Eeprom = new DeviceMemoryInfo(MemoryType.Eeprom);
            LockBits = new DeviceMemoryInfo(MemoryType.LockBits);
            FuseBits = new DeviceMemoryInfo(MemoryType.FuseBits);
        }
    }
}
