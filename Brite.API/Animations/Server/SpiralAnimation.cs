﻿using Brite.API.Animations.Commands;
using Brite.Utility.IO;
using System;
using System.Threading.Tasks;

namespace Brite.API.Animations.Server
{
    public class SpiralAnimation : BaseAnimation
    {
        public override string GetName()
        {
            return "Spiral";
        }

        public override Type GetAnimation()
        {
            return typeof(Brite.Animations.SpiralAnimation);
        }

        public override async Task HandleRequestAsync(Channel channel, BinaryStream inputStream, BinaryStream outputStream)
        {
            var anim = channel.Animation as Brite.Animations.SpiralAnimation;
            if (anim == null)
                throw new InvalidOperationException("Received request for invalid animation");

            var command = await inputStream.ReadUInt8Async();
            if (command == (byte)Spiral.SetForwardEnabled)
            {
                var forward = await inputStream.ReadBooleanAsync();
                await anim.SetAsForwardAsync(forward);
            }
            else if (command == (byte)Spiral.SetGroupSize)
            {
                var size = await inputStream.ReadUInt16Async();
                await anim.SetGroupSizeAsync(size);
            }
        }
    }
}