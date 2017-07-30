using System.Threading.Tasks;

namespace Brite.Animations
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

        public override async Task SetColorCount(byte colorCount)
        {
            await base.SetColorCount((byte)(colorCount + 1));
        }

        public override async Task SetColor(byte index, Color color)
        {
            await base.SetColor((byte)(index + 1), color);
        }

        public async Task SetBackgroundColor(Color color)
        {
            await base.SetColor(0, color);
        }

        public async Task SetAsForward(bool forward)
        {
            await SendRequest(stream =>
            {
                // Write request
                stream.WriteUInt8((byte)Command.SetForwardEnabled);
                stream.WriteBoolean(forward);
            });
        }

        public async Task SetGroupSize(ushort size)
        {
            await SendRequest(stream =>
            {
                // Write request
                stream.WriteUInt8((byte)Command.SetGroupSize);
                stream.WriteUInt16(size);
            });
        }
    }
}
