namespace Brite.Utility.IO
{
    public interface ITimedStream : IStream
    {
        int Timeout { get; set; }
    }
}
