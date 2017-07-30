namespace Brite.Micro.Protocols.AVR109.Messages
{
    internal class ReadSignatureBytesResponse : Response
    {
        internal byte[] Signature => new[] { Bytes[2], Bytes[1], Bytes[0] };
    }
}
