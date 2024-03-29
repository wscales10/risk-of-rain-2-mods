﻿using Microsoft.VisualStudio.Threading;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Async
{
	public class AsyncManager
	{
		[SuppressMessage("Minor Code Smell", "S1450:Private fields only used as local variables in methods should become local variables", Justification = "To be used in the future")]
		private readonly JoinableTaskContext joinableTaskContext;

		private readonly JoinableTaskFactory joinableTaskFactory;

		public AsyncManager()
		{
			joinableTaskContext = new JoinableTaskContext();
			joinableTaskFactory = new JoinableTaskFactory(joinableTaskContext);
		}

		internal static AsyncManager Instance { get; } = new AsyncManager();

		public static async Task WaitForAnyCompletionAsync(Task task, CancellationToken cancellationToken)
		{
			try
			{
				// TODO: Use TaskCompletionSource
				await task;
			}
			catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
			{
			}
		}

		public static async Task RunIgnoringMyTokenAsync(Func<Task> taskGetter, CancellationToken cancellationToken)
		{
			await WaitForAnyCompletionAsync(taskGetter(), cancellationToken);
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

		public JoinableTask RunSafely(Func<Task> taskGetter, CancellationToken cancellationToken = default) => joinableTaskFactory.RunAsync(() => RunIgnoringMyTokenAsync(taskGetter, cancellationToken));

		public JoinableTask<T> RunSafely<T>(Func<Task<T>> taskGetter) => joinableTaskFactory.RunAsync(() => RunIgnoringMyTokenAsync(taskGetter));

		public void RunSynchronously(Func<Task> taskGetter) => RunIgnoringMyToken(taskGetter, default);

		public T RunSynchronously<T>(Func<Task<T>> taskGetter) => RunIgnoringMyToken(taskGetter);

		public void RunIgnoringMyToken(Func<Task> taskGetter, CancellationToken cancellationToken) => joinableTaskFactory.Run(() => RunIgnoringMyTokenAsync(taskGetter, cancellationToken));

		public T RunIgnoringMyToken<T>(Func<Task<T>> taskGetter) => joinableTaskFactory.Run(() => RunIgnoringMyTokenAsync(taskGetter));

		public async Task SwitchToMainThreadAsync() => await joinableTaskFactory.SwitchToMainThreadAsync();

		private static async Task<T> RunIgnoringMyTokenAsync<T>(Func<Task<T>> taskGetter)
		{
			try
			{
				return await taskGetter();
			}
			catch (OperationCanceledException ex)
			{
				System.Diagnostics.Debugger.Break();
				throw;
			}
		}
	}
}