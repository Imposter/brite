﻿/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Threading.Tasks;

using Brite.Utility.IO;

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

        public override Task HandleRequestAsync(Channel channel, BinaryStream inputStream)
        {
            throw new NotImplementedException();
        }
    }
}
