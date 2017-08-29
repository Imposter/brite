using System;
using System.Threading.Tasks;
using Brite.Utility.Crypto;
using Brite.Utility.IO;

namespace Brite.API.Animations.Server
{
    public abstract class BaseAnimation
    {
        public abstract string GetName();
        public abstract Type GetAnimation();

        public BaseAnimation()
        {
        }

        public uint GetId()
        {
            return Hash.Fnv1A32(GetName());
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task HandleRequestAsync(Channel channel, BinaryStream inputStream, BinaryStream outputStream)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
        }
    }
}
