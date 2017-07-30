using System.Collections.Generic;
using System.Linq;
using Brite.Micro.Hardware.Memory;

namespace Brite.Micro.Hardware
{
    public abstract class Microcontroller : IMicrocontroller
    {
        // TODO: move properties (both on interface and implementation to correct corresponding places)
        // At the moment this is just one giant mixin class.
        public abstract byte DeviceCode { get; }
        public abstract byte DeviceRevision { get; }
        public abstract byte LockBytes { get; }
        public abstract byte FuseBytes { get; }

        public abstract byte Timeout { get; }
        public abstract byte StabDelay { get; }
        public abstract byte CmdExeDelay { get; }
        public abstract byte SynchLoops { get; }
        public abstract byte ByteDelay { get; }
        public abstract byte PollValue { get; }
        public abstract byte PollIndex { get; }

        public virtual byte ProgType => 0;
        public virtual byte ParallelMode => 0;
        public virtual byte Polling => 1;
        public virtual byte SelfTimed => 1;

        public abstract IDictionary<Command, byte[]> CommandBytes { get; }

        public IMemory Flash
        {
            get { return Memory.SingleOrDefault(x => x.Type == MemoryType.Flash); }
        }

        public IMemory Eeprom
        {
            get { return Memory.SingleOrDefault(x => x.Type == MemoryType.Eeprom); }
        }

        public abstract IList<IMemory> Memory { get; }

        public abstract string DeviceSignature { get; }
    }
}
