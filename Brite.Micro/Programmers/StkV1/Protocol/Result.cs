namespace Brite.Micro.Programmers.StkV1.Protocol
{
    internal enum Result : byte
    {
        NoSync,
        InSync = 0x14,
        Ok = 0x10
    }
}
