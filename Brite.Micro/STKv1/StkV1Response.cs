namespace Brite.Micro.STKv1
{
    public enum StkV1Response : byte
    {
        NoSync,
        InSync = 0x14,
        Ok = 0x10
    }
}
