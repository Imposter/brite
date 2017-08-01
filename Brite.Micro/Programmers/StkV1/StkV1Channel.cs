using Brite.Utility.IO;

namespace Brite.Micro.Programmers.StkV1
{
    public class StkV1Channel : SerialChannel
    {
        public StkV1Channel(ISerialConnection serial) 
            : base(serial, SerialPinType.Dtr)
        {
        }
    }
}
