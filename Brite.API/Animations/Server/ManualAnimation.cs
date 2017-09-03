using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brite.API.Animations.Commands;
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
            var command = await inputStream.ReadUInt8Async();
            if (command == (byte)Manual.SetColor)
            {
                // TODO: ...
            }
        }
    }
}
