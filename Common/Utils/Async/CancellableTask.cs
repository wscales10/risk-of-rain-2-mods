using Microsoft.VisualStudio.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Async
{
    public class CancellableTask
    {
        private readonly Func<CancellationToken, Task> func;

        private readonly AsyncManualResetEvent resetEvent = new AsyncManualResetEvent();

        private bool isRunning;

        public CancellableTask(Func<CancellationToken, Task> func)
        {
            this.func = func;
        }

        public Task Awaitable => resetEvent.WaitAsync();

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            if (isRunning)
            {
                throw new InvalidOperationException();
            }

            isRunning = true;
            await func(cancellationToken);
            resetEvent.Set();
        }
    }
}