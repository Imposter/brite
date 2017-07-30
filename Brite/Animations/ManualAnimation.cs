using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brite.Animations
{
    public class ManualAnimation : Animation
    {
        private enum Command : byte
        {
            SetColor,
            SetColorRange,
            SetColors,
            SetColorsRange
        }

        public override string GetName()
        {
            return "Manual";
        }

        [Obsolete]
        public override Task SetColorCount(byte colorCount)
        {
            throw new NotSupportedException("SetColorCount is not supported by this animation");
        }

        [Obsolete]
        public override Task SetColor(byte index, Color color)
        {
            throw new NotSupportedException("SetColor is not supported by this animation");
        }
        
        public async Task SetColor(ushort index, Color color)
        {
            await SendRequest(stream =>
            {
                stream.WriteUInt8((byte)Command.SetColor);
                stream.WriteUInt16(index);
                stream.WriteUInt8(color.R);
                stream.WriteUInt8(color.G);
                stream.WriteUInt8(color.B);
            });
        }

        public async Task SetColor(ushort startIndex, ushort count, Color color)
        {
            await SendRequest(stream =>
            {
                stream.WriteUInt8((byte)Command.SetColorRange);
                stream.WriteUInt16(startIndex);
                stream.WriteUInt16(count);
                stream.WriteUInt8(color.R);
                stream.WriteUInt8(color.G);
                stream.WriteUInt8(color.B);
            });
        }

        public async Task SetColors(List<Color> colors)
        {
            await SendRequest(stream =>
            {
                stream.WriteUInt8((byte)Command.SetColors);
                stream.WriteUInt16((ushort)colors.Count);
                foreach (var color in colors)
                {
                    stream.WriteUInt8(color.R);
                    stream.WriteUInt8(color.G);
                    stream.WriteUInt8(color.B);
                }
            });
        }

        public async Task SetColors(ushort startIndex, List<Color> colors)
        {
            await SendRequest(stream =>
            {
                stream.WriteUInt8((byte)Command.SetColorsRange);
                stream.WriteUInt16(startIndex);
                stream.WriteUInt16((ushort)colors.Count);
                foreach (var color in colors)
                {
                    stream.WriteUInt8(color.R);
                    stream.WriteUInt8(color.G);
                    stream.WriteUInt8(color.B);
                }
            });
        }
    }
}
