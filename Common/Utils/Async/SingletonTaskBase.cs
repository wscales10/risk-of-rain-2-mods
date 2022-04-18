using Microsoft.VisualStudio.Threading;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Utils.Async
{
	public abstract class SingletonTaskBase
	{
		public virtual async Task RunAsync()
		{
			Start();
			await Task;
		}

		public Task Task { get; protected set; } = Task.CompletedTask;

		public bool IsRunning => !Task.IsCompleted;

		[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Not async")]
		protected abstract Task GetTask();

		protected virtual Task GetTaskInTheMoment() => GetTask();

		private void ThrowIfRunning()
		{
			if (IsRunning)
			{
				throw new InvalidOperationException();
			}
		}

		public void Start()
		{
			ThrowIfRunning();
			Task = GetTaskInTheMoment();
		}
	}

	public abstract class SingletonTaskWithAsyncSetupBase : SingletonTaskBase
	{
		private JoinableTask startTask;
		private readonly AsyncManager asyncManager;

		protected SingletonTaskWithAsyncSetupBase(AsyncManager asyncManager)
		{
			this.asyncManager = asyncManager;
		}

		public sealed override async Task RunAsync()
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

		protected sealed override Task GetTaskInTheMoment()
		{
			startTask = asyncManager.RunSafely(Setup);
			return startTask.Task.ContinueWith((t) => GetTask(), TaskScheduler.Default);
		}
	}

	public abstract class SingletonTaskWithSetupBase : SingletonTaskBase
	{
		protected abstract void Setup();

		protected sealed override Task GetTaskInTheMoment()
		{
			Setup();
			return base.GetTaskInTheMoment();
		}
	}
}