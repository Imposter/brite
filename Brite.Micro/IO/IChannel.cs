using System;
using System.Threading.Tasks;

namespace Brite.Micro.IO
{
    public interface IChannel : IDisposable
    {
        Task Open();
        Task Close();

        Task ToggleReset(bool val);

        Task SendByte(byte bt);
        Task<byte> ReceiveByte();

        string Name { get; }
        bool IsOpen { get; }
    }
}
