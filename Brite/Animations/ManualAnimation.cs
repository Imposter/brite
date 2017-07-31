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
            await SendRequest(async stream =>
            {
                await stream.WriteUInt8((byte)Command.SetColor);
                await stream.WriteUInt16(index);
                await stream.WriteUInt8(color.R);
                await stream.WriteUInt8(color.G);
                await stream.WriteUInt8(color.B);
            });
        }

        public async Task SetColor(ushort startIndex, ushort count, Color color)
        {
            await SendRequest(async stream =>
            {
                await stream.WriteUInt8((byte)Command.SetColorRange);
                await stream.WriteUInt16(startIndex);
                await stream.WriteUInt16(count);
                await stream.WriteUInt8(color.R);
                await stream.WriteUInt8(color.G);
                await stream.WriteUInt8(color.B);
            });
        }

        public async Task SetColors(List<Color> colors)
        {
            await SendRequest(async stream =>
            {
                await stream.WriteUInt8((byte)Command.SetColors);
                await stream.WriteUInt16((ushort)colors.Count);
                foreach (var color in colors)
                {
                    await stream.WriteUInt8(color.R);
                    await stream.WriteUInt8(color.G);
                    await stream.WriteUInt8(color.B);
                }
            });
        }

        public async Task SetColors(ushort startIndex, List<Color> colors)
        {
            await SendRequest(async stream =>
            {
                await stream.WriteUInt8((byte)Command.SetColorsRange);
                await stream.WriteUInt16(startIndex);
                await stream.WriteUInt16((ushort)colors.Count);
                foreach (var color in colors)
                {
                    await stream.WriteUInt8(color.R);
                    await stream.WriteUInt8(color.G);
                    await stream.WriteUInt8(color.B);
                }
            });
        }
    }
}
