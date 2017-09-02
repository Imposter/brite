using System;

namespace Brite.API
{
    public class BriteException : Exception
    {
        public BriteException()
        {
        }

        public BriteException(string message) 
            : base(message)
        {
        }

        public BriteException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
