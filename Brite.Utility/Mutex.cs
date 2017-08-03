using System.Threading;
using System.Threading.Tasks;

namespace Brite.Utility
{
    public class Mutex
    {
        private readonly SemaphoreSlim _semaphore;

        public Mutex()
        {
            _semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task LockAsync()
        {
            await _semaphore.WaitAsync();
        }

        public void Unlock()
        {
            _semaphore.Release();
        }
    }
}
