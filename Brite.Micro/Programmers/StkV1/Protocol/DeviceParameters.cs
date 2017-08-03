namespace Brite.Micro.Programmers.StkV1.Protocol
{
    internal struct DeviceParameters
    {
        public DeviceType DeviceCode;
        public byte Revision;
        public byte ProgramType;
        public byte ParallelMode;
        public byte Polling;
        public byte SelfTimed;
        public byte LockBytes;
        public byte FuseBytes;
        public byte FlashPollValue;
        public byte EepromPollValue;
        public ushort PageSize;
        public ushort EepromSize;
        public uint FlashSize;
    }
}
