/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

namespace Brite
{
    public class Color
    {
        public static readonly Color Black = new Color(0, 0, 0);

        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public Color()
        {
            
        }

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color(int r, int g, int b)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
        }
    }
}
