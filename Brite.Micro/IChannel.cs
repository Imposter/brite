using Brite.Utility.IO;
using System;
using System.Threading.Tasks;

namespace Brite.Micro
{
    public interface IChannel : IDisposable
    {
        bool IsOpen { get; }
        BinaryStream Stream { get; }

        Task Open();
        Task Close();

        Task ToggleReset(bool reset);
    }
}
