using System;
using System.Threading;
using System.Threading.Tasks;

namespace Brite.Utility
{
    public static class TaskExtensions
    {
        // http://www.engineerspock.com/2016/01/22/make-your-async-code-cancellable/
        // ReSharper disable once AsyncConverter.AsyncMethodNamingHighlighting
        // ReSharper disable once ConsiderUsingAsyncSuffix
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(
                s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            return await task;
        }

        // ReSharper disable once AsyncConverter.AsyncMethodNamingHighlighting
        // ReSharper disable once ConsiderUsingAsyncSuffix
        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(
                s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            await task;
        }
    }
}
