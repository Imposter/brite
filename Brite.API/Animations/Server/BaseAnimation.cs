using Brite.Utility.Crypto;
using Brite.Utility.IO;

namespace Brite.API.Animations.Server
{
    public abstract class BaseAnimation
    {
        private readonly BriteServer _server;

        public abstract string GetName();

        public BaseAnimation(BriteServer server)
        {
            _server = server;
        }

        public uint GetId()
        {
            return Hash.Fnv1A32(GetName());
        }

        public virtual void HandleRequest(Channel channel, IStream inputStream, IStream outputStream)
        {
        }
    }
}
