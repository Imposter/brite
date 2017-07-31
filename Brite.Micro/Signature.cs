namespace Brite.Micro
{
    public struct Signature
    {
        public Signature(byte vendor, byte middle, byte low)
        {
            Vendor = vendor;
            Middle = middle;
            Low = low;
        }

        public byte Vendor;
        public byte Middle;
        public byte Low;

        public static Signature Parse(int val)
        {
            return new Signature((byte)(val >> 16), (byte)(val >> 8), (byte)val);
        }
    }
}
