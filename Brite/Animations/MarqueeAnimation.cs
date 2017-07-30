using System.Threading.Tasks;

namespace Brite.Animations
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
            await SendRequest(stream =>
            {
                // Write request
                stream.WriteUInt8((byte)Command.SetForwardEnabled);
                stream.WriteBoolean(forward);
            });
        }
    }
}
