using System.Collections.Generic;
using Brite.Micro.Hardware.Memory;

namespace Brite.Micro.Hardware
{
    public interface IMicrocontroller
    {
        byte DeviceCode { get; }
        byte DeviceRevision { get; }
        byte ProgType { get; }
        byte ParallelMode { get; }
        byte Polling { get; }
        byte SelfTimed { get; }
        byte LockBytes { get; }
        byte FuseBytes { get; }

        byte Timeout { get; }
        byte StabDelay { get; }
        byte CmdExeDelay { get; }
        byte SynchLoops { get; }
        byte ByteDelay { get; }
        byte PollValue { get; }
        byte PollIndex { get; }

        IDictionary<Command, byte[]> CommandBytes { get; }

        IMemory Flash { get; }
        IMemory Eeprom { get; }

        IList<IMemory> Memory { get; }

        string DeviceSignature { get; }
    }
}
