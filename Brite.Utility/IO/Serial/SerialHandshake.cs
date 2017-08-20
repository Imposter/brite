using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brite.Utility.IO.Serial
{
    public enum SerialHandshake
    {
        None = 0,
        RequestToSend = 1,
        XOnXOff = 2,
        RequestToSendXOnXOff = 3
    }
}
