using Brite.Utility.IO;
using System;
using System.Threading.Tasks;

namespace Brite.API.Animations.Server
{
    public class BreatheAnimation : BaseAnimation
    {
        public override string GetName()
        {
            return "Breathe";
        }

        public override Type GetAnimation()
        {
            return typeof(Brite.Animations.BreatheAnimation);
        }

        public override Task HandleRequestAsync(Channel channel, BinaryStream inputStream, BinaryStream outputStream)
        {
            throw new NotImplementedException();
        }
    }
}
