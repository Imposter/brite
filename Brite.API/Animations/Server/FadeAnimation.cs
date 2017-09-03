using Brite.Utility.IO;
using System;
using System.Threading.Tasks;

namespace Brite.API.Animations.Server
{
    public class FadeAnimation : BaseAnimation
    {
        public override string GetName()
        {
            return "Fade";
        }

        public override Type GetAnimation()
        {
            return typeof(Brite.Animations.FadeAnimation);
        }

        public override Task HandleRequestAsync(Channel channel, BinaryStream inputStream, BinaryStream outputStream)
        {
            throw new NotImplementedException();
        }
    }
}
