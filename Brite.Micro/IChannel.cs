using System;
using System.Threading.Tasks;
using Brite.Utility.IO;

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
