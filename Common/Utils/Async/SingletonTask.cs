using System;
using System.Threading.Tasks;

namespace Utils.Async
{
	public class SingletonTask : SingletonTaskBase
	{
		private readonly Func<Task> taskGetter;

		public SingletonTask(Func<Task> taskGetter)
		{
			this.taskGetter = taskGetter;
		}

		protected override Task GetTask() => taskGetter();
	}

	public class SingletonTaskWithSetup : SingletonTaskWithSetupBase
	{
		private readonly Func<Task> taskGetter;
		private readonly Action setupGetter;

		public SingletonTaskWithSetup(Func<Task> taskGetter)
		{
			this.taskGetter = taskGetter;
		}

		public SingletonTaskWithSetup(Action setupGetter, Func<Task> taskGetter) : this(taskGetter)
		{
			this.setupGetter = setupGetter;
		}

		protected override Task GetTask() => taskGetter();

		protected override void Setup() => setupGetter();
	}

	public class SingletonTaskWithAsyncSetup : SingletonTaskWithAsyncSetupBase
	{
		private readonly Func<Task> taskGetter;
		private readonly Func<Task> setupGetter;

		public SingletonTaskWithAsyncSetup(Func<Task> taskGetter, AsyncManager asyncManager) : base(asyncManager)
		{
			this.taskGetter = taskGetter;
		}

		public SingletonTaskWithAsyncSetup(Func<Task> setupGetter, Func<Task> taskGetter, AsyncManager asyncManager) : this(taskGetter, asyncManager)
		{
			this.setupGetter = setupGetter;
		}

		protected override Task GetTask() => taskGetter();

		protected override Task Setup() => setupGetter is null ? Task.CompletedTask : setupGetter();
	}
}