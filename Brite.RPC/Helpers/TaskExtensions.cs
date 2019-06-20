using System;
using System.Threading;
using System.Threading.Tasks;

namespace Brite.RPC.Helpers
{
    internal static class TaskExtensions
    {
        // https://stackoverflow.com/questions/4238345/asynchronously-wait-for-taskt-to-complete-with-timeout
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask != task) throw new TimeoutException("The operation has timed out");
                timeoutCancellationTokenSource.Cancel();
                return await task;
            }
        }

        public static TResult WaitForResult<TResult>(this Task<TResult> task)
        {
            task.Wait();
            if (task.Exception != null)
                throw task.Exception;
            return task.Result;
        }
    }
}
