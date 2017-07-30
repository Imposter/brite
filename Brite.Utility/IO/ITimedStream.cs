using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public interface ITimedStream : IStream
    {
        int Timeout { get; set; }
    }
}
