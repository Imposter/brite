using System;
using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public interface ISerialConnection : IDisposable
    {
        string PortName { get; set; }
        uint BaudRate { get; set; }
        int Timeout { get; set; }
        bool DtrEnable { get; set; }
        bool RtsEnable { get; set; }
        ushort DataBits { get; set; }
        SerialStopBits StopBits { get; set; }
        SerialParity Parity { get; set; }
        IStream BaseStream { get; }
        bool IsOpen { get; }

        Task Open();
        Task Close();
    }
}
