using System;

namespace Brite.Micro
{
    public class ProtocolException : Exception
    {
        public ProtocolException(string message)
            : base(message)
        {
        }
    }
}
