using System.Reactive.Concurrency;

namespace Brite.App.Win.Services
{
    public interface ISchedulerService : IService
    {
        IScheduler Dispatcher { get; }

        IScheduler Current { get; }

        IScheduler TaskPool { get; }

        IScheduler EventLoop { get; }

        IScheduler NewThread { get; }

        IScheduler StaThread { get; }
    }
}
