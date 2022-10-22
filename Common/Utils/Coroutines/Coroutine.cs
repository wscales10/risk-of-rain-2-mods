using System;
using System.Collections;

namespace Utils.Coroutines
{
	public delegate bool ProgressHandler(object progressUpdate);

	public delegate IEnumerable CoroutineMethod(Reference reference);

	public class CoroutineRun
	{
		private readonly CoroutineMethod func;

		public CoroutineRun(CoroutineMethod func)
		{
			this.func = func ?? throw new ArgumentNullException(nameof(func));
		}

		public bool Continue { get; set; }

		public Result Result { get; private set; }

		public IEnumerable Invoke()
		{
			var reference = new Reference();

			foreach (object progressUpdate in func(reference))
			{
				if (reference.IsFinished)
				{
					Result = new Result(reference);
					yield break;
				}

				yield return progressUpdate;

				if (!Continue)
				{
					reference.Cancel();
				}

				if (reference.IsFinished)
				{
					Result = new Result(reference);
					yield break;
				}
			}

			if (reference.IsFinished)
			{
				Result = new Result(reference);
			}
			else
			{
				throw new InvalidOperationException("Iterator ended without finalizing reference");
			}
		}
	}

	public class Coroutine2
	{
		private readonly Func<CoroutineMethod> getter;

		public Coroutine2(Func<CoroutineMethod> getter) => this.getter = getter;

		public CoroutineRun Instantiate()
		{
			return new CoroutineRun(getter());
		}
	}

	public class Coroutine
	{
		private readonly Func<CoroutineMethod> getter;

		public Coroutine(Func<CoroutineMethod> getter) => this.getter = getter;

		public static Result Run(Func<CoroutineMethod> getter, ProgressHandler handler = null)
		{
			return new Coroutine(getter).Invoke(handler);
		}

		public Result Invoke(ProgressHandler handler)
		{
			CoroutineMethod func = getter();

			if (func is null)
			{
				return Result.Faulted;
			}

			var reference = new Reference();

			foreach (object progressUpdate in func(reference))
			{
				if (reference.IsFinished)
				{
					return new Result(reference);
				}

				if (!(handler?.Invoke(progressUpdate) ?? true))
				{
					reference.Cancel();
				}

				if (reference.IsFinished)
				{
					return new Result(reference);
				}
			}

			if (reference.IsFinished)
			{
				return new Result(reference);
			}
			else
			{
				throw new InvalidOperationException("Iterator ended without finalizing reference");
			}
		}
	}
}