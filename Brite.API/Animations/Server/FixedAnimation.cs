using Brite.Utility.IO;
using System;
using System.Threading.Tasks;

namespace Brite.API.Animations.Server
{
    public class FixedAnimation : BaseAnimation
    {
        public override string GetName()
        {
            return "Fixed";
        }

        public override Type GetAnimation()
        {
            return typeof(Brite.Animations.FixedAnimation);
        }

        public override Task HandleRequestAsync(Channel channel, BinaryStream inputStream, BinaryStream outputStream)
        {
            throw new NotImplementedException();
        }
    }
}
