namespace Brite.Micro.Hardware.Memory
{
    public interface IMemory
    {
        MemoryType Type { get; }
        int Size { get; }
        int PageSize { get; }
        byte PollVal1 { get; }
        byte PollVal2 { get; }
        byte Delay { get; }
        byte[] CmdBytesRead { get; }
        byte[] CmdBytesWrite { get; }
    }
}
