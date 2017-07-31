using System.Threading.Tasks;

namespace Brite.Micro
{
    public abstract class ProgrammerOperation : IProgrammerOperation
    {
        private IProgrammer _programmer;

        public void Initialize(IProgrammer programmer)
        {
            _programmer = programmer;
        }

        public abstract Task Execute();
    }
}
