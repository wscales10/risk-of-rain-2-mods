﻿using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;

namespace Utils
{
    public abstract class AsyncRunner
    {
        private readonly RunStateHolder runStateHolder = new RunStateHolder();

        private readonly AsyncSemaphore asyncSemaphore = new AsyncSemaphore(1);

        protected AsyncRunner() => Info = runStateHolder.ToReadOnly();

        public ReadOnlyRunStateHolder Info { get; }

        public async Task<bool> TryStopAsync()
        {
            using (await asyncSemaphore.EnterAsync())
            {
                if (runStateHolder.IsOn)
                {
                    await StopAsync();
                    runStateHolder.State = RunState.Off;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> TryStartAsync()
        {
            using (await asyncSemaphore.EnterAsync())
            {
                if (!runStateHolder.IsOn)
                {
                    await StartAsync();
                    runStateHolder.State = RunState.Running;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> TryResumeAsync()
        {
            using (await asyncSemaphore.EnterAsync())
            {
                if (runStateHolder.IsPaused)
                {
                    await ResumeAsync();
                    runStateHolder.State = RunState.Running;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> TryPauseAsync()
        {
            using (await asyncSemaphore.EnterAsync())
            {
                if (runStateHolder.IsRunning)
                {
                    await PauseAsync();
                    runStateHolder.State = RunState.Paused;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        protected abstract Task StopAsync();

        protected abstract Task StartAsync();

        protected virtual Task ResumeAsync() => StartAsync();

        protected abstract Task PauseAsync();
    }
}