using System;
using Brite.Utility.IO;

namespace Brite.Micro
{
    public static class LogExtensions
    {
        public static void ThrowError(this Log log, string format, params object[] args)
        {
            string message = string.Format(format, args);
            log.Error(message);
            throw new Exception(message);
        }
    }
}
