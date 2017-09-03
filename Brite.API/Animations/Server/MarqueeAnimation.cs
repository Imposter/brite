using Brite.API.Animations.Commands;
using Brite.Utility.IO;
using System;
using System.Threading.Tasks;

namespace Brite.API.Animations.Server
{
    public class MarqueeAnimation : BaseAnimation
    {
        public override string GetName()
        {
            return "Marquee";
        }

        public override Type GetAnimation()
        {
            return typeof(Brite.Animations.MarqueeAnimation);
        }

        public override async Task HandleRequestAsync(Channel channel, BinaryStream inputStream, BinaryStream outputStream)
        {
            var anim = channel.Animation as Brite.Animations.MarqueeAnimation;
            if (anim == null)
                throw new InvalidOperationException("Received request for invalid animation");

            var command = await inputStream.ReadUInt8Async();
            if (command == (byte) Marquee.SetForwardEnabled)
            {
                var forward = await inputStream.ReadBooleanAsync();
                await anim.SetAsForwardAsync(forward);
            }
        }
    }
}
