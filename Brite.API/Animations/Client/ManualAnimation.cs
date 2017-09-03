using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brite.API.Animations.Commands;

namespace Brite.API.Animations.Client
{
    public class ManualAnimation : BaseAnimation
    {
        public override string GetName()
        {
            return "Manual";
        }

        [Obsolete]
        public override Task SetColorCountAsync(byte colorCount)
        {
            throw new NotSupportedException("SetColorCount is not supported by this animation");
        }

        [Obsolete]
        public override Task SetColorAsync(byte index, Color color)
        {
            throw new NotSupportedException("SetColor is not supported by this animation");
        }

        public async Task SetColorAsync(ushort index, Color color)
        {
            await SendRequestAsync(async stream =>
            {
                await stream.WriteUInt8Async((byte)Manual.SetColor);
                await stream.WriteUInt16Async(index);
                await stream.WriteUInt8Async(color.R);
                await stream.WriteUInt8Async(color.G);
                await stream.WriteUInt8Async(color.B);
            });
        }

        public async Task SetColorAsync(ushort startIndex, ushort count, Color color)
        {
            await SendRequestAsync(async stream =>
            {
                await stream.WriteUInt8Async((byte)Manual.SetColorRange);
                await stream.WriteUInt16Async(startIndex);
                await stream.WriteUInt16Async(count);
                await stream.WriteUInt8Async(color.R);
                await stream.WriteUInt8Async(color.G);
                await stream.WriteUInt8Async(color.B);
            });
        }

        public async Task SetColorsAsync(List<Color> colors)
        {
            await SendRequestAsync(async stream =>
            {
                await stream.WriteUInt8Async((byte)Manual.SetColors);
                await stream.WriteUInt16Async((ushort)colors.Count);
                foreach (var color in colors)
                {
                    await stream.WriteUInt8Async(color.R);
                    await stream.WriteUInt8Async(color.G);
                    await stream.WriteUInt8Async(color.B);
                }
            });
        }

        public async Task SetColorsAsync(ushort startIndex, List<Color> colors)
        {
            await SendRequestAsync(async stream =>
            {
                await stream.WriteUInt8Async((byte)Manual.SetColorsRange);
                await stream.WriteUInt16Async(startIndex);
                await stream.WriteUInt16Async((ushort)colors.Count);
                foreach (var color in colors)
                {
                    await stream.WriteUInt8Async(color.R);
                    await stream.WriteUInt8Async(color.G);
                    await stream.WriteUInt8Async(color.B);
                }
            });
        }
    }
}
