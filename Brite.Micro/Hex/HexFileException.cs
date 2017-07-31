using System;

namespace Brite.Micro.Hex
{
    public class HexFileException : Exception
    {
        public HexFileException(string message)
          : base(message)
        {
        }
    }
}
