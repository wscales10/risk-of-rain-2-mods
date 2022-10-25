using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Utils.Coroutines
{
	public delegate bool ProgressHandler(object progressUpdate);

	public delegate IEnumerable<ProgressUpdate> CoroutineMethod(Reference reference);

	[Serializable]
	public class FailedCoroutineRunException : Exception
	{
		public FailedCoroutineRunException()
		{
		}

		public FailedCoroutineRunException(string message) : base(message)
		{
		}

		public FailedCoroutineRunException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected FailedCoroutineRunException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}

	public class ProgressUpdate
	{
		public ProgressUpdate(object sender, object args)
		{
			Sender = sender;
			Args = args;
		}

		public object Sender { get; }

		public object Args { get; }
	}

	public class CoroutineRun
	{
		private readonly CoroutineMethod func;

		public CoroutineRun(CoroutineMethod func)
		{
			this.func = func ?? throw new ArgumentNullException(nameof(func));
		}

		public bool Continue { get; set; } = true;

		public Result Result { get; private set; }

		public CoroutineRun RunToCompletion()
		{
			_ = GetProgressUpdates().LastOrDefault();
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

		public IEnumerable<ProgressUpdate> GetProgressUpdates()
		{
			var reference = new Reference();

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

	public class Coroutine
	{
		private readonly CoroutineMethod method;

		public Coroutine(CoroutineMethod method) => this.method = method;

		public static IEnumerable<ProgressUpdate> DefaultMethod(Reference reference)
		{
			reference.Complete();
			yield break;
		}

		public CoroutineRun CreateRun()
		{
			return new CoroutineRun(method);
		}
	}
}