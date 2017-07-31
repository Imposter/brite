namespace Brite.Micro.Programmers.StkV1.Protocol
{
    internal struct DeviceParameters
    {
        public DeviceType DeviceCode;
        public byte Revision;
        public byte ProgType;
        public byte ParMode;
        public byte Polling;
        public byte SelfTimed;
        public byte LockBytes;
        public byte FuseBytes;
        public byte FlashPollVal1;
        public byte FlashPollVal2;
        public byte EepromPollVal1;
        public byte EepromPollVal2;
        public ushort PageSize;
        public ushort EepromSize;
        public uint FlashSize;
    }
}
