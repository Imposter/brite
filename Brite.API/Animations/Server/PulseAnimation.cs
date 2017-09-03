using Brite.Utility.IO;
using System;
using System.Threading.Tasks;

namespace Brite.API.Animations.Server
{
    public class PulseAnimation : BaseAnimation
    {
        public override string GetName()
        {
            return "Pulse";
        }

        public override Type GetAnimation()
        {
            return typeof(Brite.Animations.PulseAnimation);
        }

        public override Task HandleRequestAsync(Channel channel, BinaryStream inputStream, BinaryStream outputStream)
        {
            throw new NotImplementedException();
        }
    }
}
