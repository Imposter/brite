namespace Brite.Micro.Programmers.StkV1
{
    public class DeviceBit
    {
        private int _address;
        private int _value;

        public int Address => _address;
        public int Value
        {
            get => _value;
            set => _value = value;
        }

        public DeviceBit(int address)
        {
            _address = address;
        }
    }
}