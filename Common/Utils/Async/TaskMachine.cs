using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utils.Async
{
	public class TaskMachine
	{
		private readonly Queue<Func<Task>> tasks = new Queue<Func<Task>>();

		private readonly SingletonTask lifecycle;

		public TaskMachine()
		{
			lifecycle = new SingletonTask(ExecuteAsync);
		}

		public Task Lifecycle => lifecycle.Task;

		public bool IsRunning => lifecycle.IsRunning;

		public void Ingest(Func<Task> task)
		{
			tasks.Enqueue(task);

			if (tasks.Count == 1)
			{
				lifecycle.Start();
			}
		}

		private async Task ExecuteAsync()
		{
			this.Log(lifecycle.Task.Status);
			while (tasks.Count > 0)
			{
				await tasks.Peek()();
				tasks.Dequeue();
			}
		}
	}
}