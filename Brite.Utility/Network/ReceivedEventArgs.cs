using System;

namespace Brite.Utility.Network
{
    public class ReceivedEventArgs : EventArgs
    {
        public byte[] Buffer { get; }
        public int Length { get; }

        public ReceivedEventArgs(byte[] buffer, int length)
        {
            Buffer = buffer;
            Length = length;
        }
    }
}
