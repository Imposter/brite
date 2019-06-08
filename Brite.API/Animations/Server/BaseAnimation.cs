/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Threading.Tasks;

using Brite.Utility.Crypto;
using Brite.Utility.IO;

namespace Brite.API.Animations.Server
{
    public abstract class BaseAnimation
    {
        public abstract string GetName();
        public abstract Type GetAnimation();

        public uint GetId()
        {
            return Hash.Fnv1A32(GetName());
        }
        
        public abstract Task HandleRequestAsync(Channel channel, BinaryStream inputStream);
    }
}
