using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Utils.Async
{
	public delegate Task CancellableTask(CancellationToken cancellationToken);

	public class TaskMachine
	{
		private readonly CancellationToken cancellationToken;

		private readonly Channel<CancellableTask> tasks = Channel.CreateUnbounded<CancellableTask>();

		public TaskMachine(CancellationToken cancellationToken)
		{
			this.cancellationToken = cancellationToken;
			Lifecycle = ExecuteAsync();
		}

		public Task Lifecycle { get; }

		public void Close()
		{
			tasks.Writer.Complete();
		}

		public bool TryIngest(CancellableTask task) => tasks.Writer.TryWrite(task);

		private async Task ExecuteAsync()
		{
			while (await tasks.Reader.WaitToReadAsync(cancellationToken))
			{
				CancellableTask cancellableTask = await tasks.Reader.ReadAsync(cancellationToken);
				MaybeCancel();
				if (!cancellationToken.IsCancellationRequested)
				{
					await cancellableTask(cancellationToken);
				}
				MaybeCancel();
			}

			void MaybeCancel()
			{
				if (cancellationToken.IsCancellationRequested)
				{
					throw new OperationCanceledException(cancellationToken);
				}
			}
		}
	}
}