using System.Threading;
using System.Threading.Tasks;
using Utils.Runners;

namespace Utils.Async
{
	public class AsyncJobQueue : Runner
	{
		private readonly object _locker = new object();

		private Task _previousJob = Task.CompletedTask;

		private int _count;

		public AsyncJobQueue(RunState initialState = RunState.Running) : base(initialState, true)
		{
		}

		public int Count => _count;

		/// <summary>
		/// Serialize jobs execution in order of arrival. Jobs are not started until the previous
		/// one is complete.
		/// </summary>
		public async Task<bool> WaitForMyJobAsync(CancellableTask cancellableTask, CancellationToken cancellationToken = default)
		{
			if (FirstBit(out TaskCompletionSource<bool> myJobIsCompleteTcs, out Task previousJob))
			{
				await SecondBitAsync(cancellableTask, previousJob, myJobIsCompleteTcs, cancellationToken);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Serialize jobs execution in order of arrival. Jobs are not started until the previous
		/// one is complete.
		/// </summary>
		public bool QueueJob(CancellableTask cancellableTask, CancellationToken cancellationToken = default)
		{
			if (FirstBit(out TaskCompletionSource<bool> myJobIsCompleteTcs, out Task previousJob))
			{
				_ = SecondBitAsync(cancellableTask, previousJob, myJobIsCompleteTcs, cancellationToken);
				return true;
			}

			return false;
		}

		protected override void Pause() => QueueJob(new CancellableTask(async _ => await Info.WaitUntilRunningAsync()));

		private bool FirstBit(out TaskCompletionSource<bool> myJobIsCompleteTcs, out Task previousJob)
		{
			if (!Info.IsOn)
			{
				myJobIsCompleteTcs = null;
				previousJob = null;
				return false;
			}

			myJobIsCompleteTcs = new TaskCompletionSource<bool>();
			lock (_locker)
			{
				// Replace the previous job with a TCS that will complete when our job completes:
				previousJob = _previousJob;
				_previousJob = myJobIsCompleteTcs.Task;
				Interlocked.Increment(ref _count); // Keep count for debug
			}

			return true;
		}

		private async Task SecondBitAsync(CancellableTask cancellableTask, Task previousJob, TaskCompletionSource<bool> myJobIsCompleteTcs, CancellationToken cancellationToken)
		{
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