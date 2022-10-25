using System;
using System.Threading.Tasks;

namespace Utils.Coroutines
{
	public class Result
	{
		public Result(Reference reference)
		{
			if (!reference.IsFinished)
			{
				throw new ArgumentOutOfRangeException(nameof(reference), reference, "Cannot create result from unfinished reference");
			}

			Status = reference.Status;
			Value = reference.Value;
		}

		private Result(TaskStatus status, object value)
		{
			Status = status;
			Value = value;
		}

		public static Result Faulted { get; } = new Result(TaskStatus.Faulted, null);

		public TaskStatus Status { get; }

		public object Value { get; }

		public bool Success => Status == TaskStatus.RanToCompletion;

		public static implicit operator bool(Result r) => r.Success;
	}
}