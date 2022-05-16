using Microsoft.VisualStudio.Threading;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Utils.Async
{
    public abstract class SingletonTaskBase
    {
        private readonly AsyncManualResetEvent resetEvent = new AsyncManualResetEvent();

        public Task Task { get; protected set; } = Task.CompletedTask;

        public bool IsRunning => !Task.IsCompleted;

        public virtual async Task RunAsync()
        {
            Start();
            await Task;
        }

        public void Start()
        {
            ThrowIfRunning();
            Task = resetEvent.WaitAsync();
            _ = SetupAsync();
        }

        [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Not async")]
        protected abstract Task GetTask();

        protected virtual Task GetTaskAsync() => GetTask();

        private async Task SetupAsync()
        {
            try
            {
                await GetTaskAsync();
            }
            catch (OperationCanceledException e)
            {
                resetEvent.
            }
            resetEvent.Set();
        }

        private void ThrowIfRunning()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException();
            }
        }
    }

    public abstract class SingletonTaskWithAsyncSetupBase : SingletonTaskBase
    {
        private readonly AsyncManager asyncManager;

        private JoinableTask startTask;

        protected SingletonTaskWithAsyncSetupBase(AsyncManager asyncManager)
        {
            this.asyncManager = asyncManager;
        }

        public override sealed async Task RunAsync()
        {
            await StartAsync();
            await Task;
        }

        public async Task StartAsync()
        {
            Start();
            await startTask;
        }

        [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Not async")]
        protected abstract Task Setup();

        protected override sealed Task GetTaskAsync()
        {
            startTask = asyncManager.RunSafely(Setup);
            return startTask.Task.ContinueWith((t) => GetTask(), TaskScheduler.Default);
        }
    }

    public abstract class SingletonTaskWithSetupBase : SingletonTaskBase
    {
        protected abstract void Setup();

        protected override sealed Task GetTaskAsync()
        {
            Setup();
            return base.GetTaskAsync();
        }
    }
}