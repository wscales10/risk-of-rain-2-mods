using Microsoft.VisualStudio.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Async
{
	public class AsyncManager
	{
		private readonly JoinableTaskContext joinableTaskContext;

		private readonly JoinableTaskFactory joinableTaskFactory;

		public AsyncManager()
		{
			joinableTaskContext = new JoinableTaskContext();
			joinableTaskFactory = new JoinableTaskFactory(joinableTaskContext);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "Helper method")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Implied")]
		public Task<T> MakeCancellable<T>(Task<T> task, CancellationToken token)
		{
			if (!token.CanBeCanceled)
			{
				return task;
			}

			var tcs = new TaskCompletionSource<T>();

			// This cancels the returned task:
			// 1. If the token has been canceled, it cancels the TCS straightaway
			// 2. Otherwise, it attempts to cancel the TCS whenever the token indicates cancelled
			token.Register(() => tcs.TrySetCanceled(token),
				useSynchronizationContext: false);

			_ = task.ContinueWith(t =>
			{
				// Complete the TCS per task status If the TCS has been cancelled, this continuation
				// does nothing
				if (task.IsCanceled)
				{
					tcs.TrySetCanceled();
				}
				else if (task.IsFaulted)
				{
					tcs.TrySetException(t.Exception);
				}
				else
				{
					tcs.TrySetResult(t.Result);
				}
			},
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);

			return tcs.Task;
		}

		public JoinableTask RunSafely(Func<Task> taskGetter)
		{
			return joinableTaskFactory.RunAsync(taskGetter);
		}

		public SingletonTaskWithAsyncSetup CreateSingletonTask(Func<Task> taskGetter) => new SingletonTaskWithAsyncSetup(taskGetter, this);

		public SingletonTaskWithAsyncSetup CreateSingletonTask(Func<Task> setupGetter, Func<Task> taskGetter) => new SingletonTaskWithAsyncSetup(setupGetter, taskGetter, this);

		public async Task SwitchToMainThreadAsync() => await joinableTaskFactory.SwitchToMainThreadAsync();
	}
}