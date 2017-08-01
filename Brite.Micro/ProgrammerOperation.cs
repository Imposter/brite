using System.Threading.Tasks;

namespace Brite.Micro
{
    public abstract class ProgrammerOperation : IProgrammerOperation
    {
        protected IProgrammer Programmer { get; }

        public ProgrammerOperation(IProgrammer programmer)
        {
            Programmer = programmer;
        }

        public abstract Task Execute();

        public virtual void Dispose()
        {
        }
    }
}
