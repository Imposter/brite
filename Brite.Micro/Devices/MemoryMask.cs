namespace Brite.Micro.Devices
{
    public class MemoryMask
    {
        public MemoryType MemoryType { get; set; }
        public int WordSize { get; set; }
        public int AddressStart { get; set; }
        public int? AddressEnd { get; set; }
        public uint Mask { get; set; }

        public bool Process(int address, MemoryType memType, ref byte bt)
        {
            if (memType != MemoryType) return false;
            if (address < AddressStart) return false;
            if (AddressEnd.HasValue)
            {
                if (address > AddressEnd.Value) return false;
            }
            else
            {
                if (address > (AddressStart + WordSize - 1)) return false;
            }
            switch (WordSize)
            {
                case 1:
                    bt = (byte)(bt & Mask);
                    return true;
                case 2:
                    if ((address & 0x01) == 0)
                    {
                        bt = (byte)(bt & Mask);
                    }
                    else
                    {
                        bt = (byte)(bt & Mask >> 8);
                    }
                    return true;
                default:
                    return false;
            }
        }
    }
}
