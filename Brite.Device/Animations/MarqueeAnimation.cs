using System.Threading.Tasks;

namespace Brite.Device.Animations
{
    public class MarqueeAnimation : Animation
    {
        private enum Command : byte
        {
            SetForwardEnabled
        }

        public override string GetName()
        {
            return "Marquee";
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
    }
}
