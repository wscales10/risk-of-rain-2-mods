using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Coroutines
{
	public class CoroutineRun
	{
		private readonly CoroutineMethod func;

		private readonly object parameter;

		public CoroutineRun(CoroutineMethod func, object parameter = null)
		{
			this.func = func ?? throw new ArgumentNullException(nameof(func));
			this.parameter = parameter;
		}

		public delegate bool ProgressUpdateHandler(ProgressUpdate progressUpdate);

		public bool Continue { get; set; } = true;

		public Result Result { get; private set; }

		public CoroutineRun RunToCompletion(bool propagateExceptions = false)
		{
			if (propagateExceptions)
			{
				foreach (var progressUpdate in GetProgressUpdates())
				{
					if (progressUpdate.Args is Exception ex)
					{
						throw ex;
					}
				}
			}
			else
			{
				_ = GetProgressUpdates().LastOrDefault();
			}

			return this;
		}

		public CoroutineRun ThrowOnFailure()
		{
			RunToCompletion();
			if (Result)
			{
				return this;
			}
			else
			{
				throw new FailedCoroutineRunException();
			}
		}

		public CoroutineRun Run(Func<ProgressUpdate, bool> handler)
		{
			foreach (var progressUpdate in GetProgressUpdates())
			{
				Continue = handler(progressUpdate);
			}

			return this;
		}

		public IEnumerable<ProgressUpdate> GetProgressUpdates()
		{
			var reference = new Reference(parameter);

			foreach (var progressUpdate in func(reference))
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
}