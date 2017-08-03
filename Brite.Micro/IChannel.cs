using Brite.Utility.IO;
using System;
using System.Threading.Tasks;

namespace Brite.Micro
{
    public interface IChannel : IDisposable
    {
        bool IsOpen { get; }
        BinaryStream Stream { get; }

        Task OpenAsync();
        Task CloseAsync();

        Task ToggleResetAsync(bool reset);
    }
}
