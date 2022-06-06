using System.Threading;
using System.Threading.Tasks;

namespace Utils.Async
{
    public class AsyncJobQueue
    {
        private readonly object _locker = new object();

        private Task _previousJob = Task.CompletedTask;

        private int _count;

        public int Count => _count;

        /// <summary>
        /// Serialize jobs execution in order of arrival. Jobs are not started until the previous
        /// one is complete.
        /// </summary>
        public async Task WaitForMyJobAsync(CancellableTask cancellableTask, CancellationToken cancellationToken = default)
        {
            Task previousJob;
            var myJobIsCompleteTcs = new TaskCompletionSource<bool>();

            lock (_locker)
            {
                // Replace the previous job with a TCS that will complete when our job completes:
                previousJob = _previousJob;
                _previousJob = myJobIsCompleteTcs.Task;
                Interlocked.Increment(ref _count); // Keep count for debug
            }

            // Wait for the previous job to complete. No need for a try catch because the previous
            // job is a TCS too, so it will never fail.
            await previousJob;

            try
            {
                await cancellableTask.RunAsync(cancellationToken);
            }
            finally
            {
                myJobIsCompleteTcs.SetResult(true);
                Interlocked.Decrement(ref _count);
            }
        }
    }
}