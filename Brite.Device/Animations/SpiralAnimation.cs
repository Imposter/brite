using System.Threading.Tasks;

namespace Brite.Device.Animations
{
    public class SpiralAnimation : Animation
    {
        private enum Command : byte
        {
            SetForwardEnabled,
            SetGroupSize
        }

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
                // WriteAsync request
                await stream.WriteUInt8Async((byte)Command.SetForwardEnabled);
                await stream.WriteBooleanAsync(forward);
            });
        }

        public async Task SetGroupSizeAsync(ushort size)
        {
            await SendRequestAsync(async stream =>
            {
                // WriteAsync request
                await stream.WriteUInt8Async((byte)Command.SetGroupSize);
                await stream.WriteUInt16Async(size);
            });
        }
    }
}
