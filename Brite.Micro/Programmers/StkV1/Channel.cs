using Brite.Utility.IO.Serial;

namespace Brite.Micro.Programmers.StkV1
{
    public class Channel : SerialChannel
    {
        public Channel(ISerialConnection serial) 
            : base(serial, SerialPinType.Dtr)
        {
        }
    }
}
