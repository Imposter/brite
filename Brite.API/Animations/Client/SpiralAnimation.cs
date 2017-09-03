using System.Threading.Tasks;
using Brite.API.Animations.Commands;

namespace Brite.API.Animations.Client
{
    public class SpiralAnimation : BaseAnimation
    {
        public override string GetName()
        {
            return "Spiral";
        }

        public override async Task SetColorCountAsync(byte colorCount)
        {
            await base.SetColorCountAsync((byte)(colorCount + 1));
        }

        public override async Task SetColorAsync(byte index, Color color)
        {
            await base.SetColorAsync((byte)(index + 1), color);
        }

        public async Task SetBackgroundColorAsync(Color color)
        {
            await base.SetColorAsync(0, color);
        }

        public async Task SetAsForwardAsync(bool forward)
        {
            await SendRequestAsync(async stream =>
            {
                // Write request
                await stream.WriteUInt8Async((byte)Spiral.SetForwardEnabled);
                await stream.WriteBooleanAsync(forward);
            });
        }

        public async Task SetGroupSizeAsync(ushort size)
        {
            await SendRequestAsync(async stream =>
            {
                // Write request
                await stream.WriteUInt8Async((byte)Spiral.SetGroupSize);
                await stream.WriteUInt16Async(size);
            });
        }
    }
}
