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

        public async Task SetAsForward(bool forward)
        {
            await SendRequest(async stream =>
            {
                // Write request
                await stream.WriteUInt8((byte)Command.SetForwardEnabled);
                await stream.WriteBoolean(forward);
            });
        }
    }
}
