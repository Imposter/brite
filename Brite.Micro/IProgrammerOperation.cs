using System;
using System.Threading.Tasks;

namespace Brite.Micro
{
    public interface IProgrammerOperation : IDisposable
    {
        Task Execute();
    }
}
