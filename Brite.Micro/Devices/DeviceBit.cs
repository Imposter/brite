using System.Globalization;

namespace Brite.Micro.Devices
{
    public class DeviceBit
    {
        public int Address { get; set; }
        public int Bit { get; set; }
        public string Name { get; set; }
        public bool Inverse { get; set; }
        public bool? Constant { get; set; }
        public bool Value { get; set; }

        public bool Hidden { get; set; }
        public string Description { get; set; }

        public byte Apply(byte value)
        {
            return Constant.HasValue ? ApplyRaw(value, Constant.Value) : ApplyRaw(value, Value ^ Inverse);
        }

        private byte ApplyRaw(byte source, bool rawValue)
        {
            var mask = 1 << Bit;
            if (rawValue)
            {
                return (byte)(source | mask);
            }
            return (byte)(source & (~mask));
        }

        public override string ToString()
        {
            return Name;
        }

        public void SetValueFrom(byte bt)
        {
            if (Constant.HasValue) return;
            var mask = 1 << Bit;
            Value = ((bt & mask) != 0) ^ Inverse;
        }

        private static int ParseInt(string val)
        {
            if (val.StartsWith("0x")) return int.Parse(val.Substring(2), NumberStyles.HexNumber);
            return int.Parse(val);
        }
    }
}
