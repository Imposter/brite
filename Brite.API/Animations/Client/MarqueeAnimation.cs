/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System.Threading.Tasks;
using Brite.API.Animations.Commands;

namespace Brite.API.Animations.Client
{
    public class MarqueeAnimation : BaseAnimation
    {
        public override string GetName()
        {
            return "Marquee";
        }

        public async Task SetAsForwardAsync(bool forward)
        {
            await SendRequestAsync(async stream =>
            {
                // Write request
                await stream.WriteUInt8Async((byte)Marquee.SetForwardEnabled);
                await stream.WriteBooleanAsync(forward);
            });
        }
    }
}
