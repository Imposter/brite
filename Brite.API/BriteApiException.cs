using System;

namespace Brite.API
{
    public class BriteApiException : Exception
    {
        public BriteApiException()
        {
        }

        public BriteApiException(string message) 
            : base(message)
        {
        }

        public BriteApiException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
