using System;

namespace Brite.RPC
{
    public sealed class RpcException : Exception
    {
        public RpcException()
        {
        }

        public RpcException(string message)
            : base(message)
        {
        }

        public RpcException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
