namespace Brite.Micro.Protocols
{
    public abstract class Response : IRequest
    {
        public byte[] Bytes { get; set; }
    }
}
