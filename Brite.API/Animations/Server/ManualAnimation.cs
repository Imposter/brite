/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using Brite.API.Animations.Commands;
using Brite.Utility.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var anim = channel.Animation as Brite.Animations.ManualAnimation;
            if (anim == null)
                throw new InvalidOperationException("Received request for invalid animation");

            var command = await inputStream.ReadUInt8Async();
            if (command == (byte)Manual.SetColor)
            {
                var index = await inputStream.ReadUInt16Async();
                var r = await inputStream.ReadUInt8Async();
                var g = await inputStream.ReadUInt8Async();
                var b = await inputStream.ReadUInt8Async();

                await anim.SetColorAsync(index, new Color(r, g, b));
            }
            else if (command == (byte) Manual.SetColorRange)
            {
                var index = await inputStream.ReadUInt16Async();
                var count = await inputStream.ReadUInt16Async();
                var r = await inputStream.ReadUInt8Async();
                var g = await inputStream.ReadUInt8Async();
                var b = await inputStream.ReadUInt8Async();

                await anim.SetColorAsync(index, count, new Color(r, g, b));
            }
            else if (command == (byte)Manual.SetColors)
            {
                var colors = new List<Color>();

                var count = await inputStream.ReadUInt16Async();
                for (var i = 0; i < count; i++)
                {
                    var r = await inputStream.ReadUInt8Async();
                    var g = await inputStream.ReadUInt8Async();
                    var b = await inputStream.ReadUInt8Async();

                    colors.Add(new Color(r, g, b));
                }

                await anim.SetColorsAsync(colors);
            }
            else if (command == (byte)Manual.SetColorsRange)
            {
                var colors = new List<Color>();

                var index = await inputStream.ReadUInt16Async();
                var count = await inputStream.ReadUInt16Async();
                for (var i = 0; i < count; i++)
                {
                    var r = await inputStream.ReadUInt8Async();
                    var g = await inputStream.ReadUInt8Async();
                    var b = await inputStream.ReadUInt8Async();

                    colors.Add(new Color(r, g, b));
                }

                await anim.SetColorsAsync(index, colors);
            }
        }
    }
}
