using System.Threading.Tasks;

namespace Brite.Micro
{
    public interface IProgrammerOperation
    {
        void Initialize(IProgrammer programmer);
        Task Execute();
    }
}
