using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brite.Utility.IO;

namespace Brite.API.Animations.Server
{
    public class ManualAnimation : BaseAnimation
    {
        public override string GetName()
        {
            return "Manual";
        }

        public override Type GetAnimation()
        {
            return typeof(Brite.Animations.ManualAnimation);
        }

        public override async Task HandleRequestAsync(Channel channel, BinaryStream inputStream, BinaryStream outputStream)
        {
            // TODO: Implement
        }
    }
}
