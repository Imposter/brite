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
        
        public abstract Task HandleRequestAsync(Channel channel, BinaryStream inputStream,
            BinaryStream outputStream);
    }
}
