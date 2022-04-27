using System;
using System.Collections.Generic;
using System.Linq;

namespace WPFApp.Controls.Wrappers
{
	public class SaveResult
	{
		public SaveResult(bool success, params Action[] release) : this(success, new Queue<Action>(release))
		{
		}

		public SaveResult(bool success, IEnumerable<Action> release)
		{
			IsSuccess = success;
			ReleaseActions = success ? new Queue<Action>(release) : new Queue<Action>();
		}

		private SaveResult(bool success, Queue<Action> release1, Queue<Action> release2) : this(success, release1.Concat(release2))
		{
		}

		public Queue<Action> ReleaseActions { get; }

		public bool IsSuccess { get; }

		public static SaveResult operator &(SaveResult result1, SaveResult result2) => new(result1.IsSuccess && result2.IsSuccess, result1.ReleaseActions, result2.ReleaseActions);

		public static SaveResult operator &(bool b, SaveResult result) => new(b && result.IsSuccess, result.ReleaseActions);

		public static bool operator true(SaveResult result) => result.IsSuccess;

		public static bool operator false(SaveResult result) => !result.IsSuccess;

		public static SaveResult<T> From<T>(T value) => new(value);

		public static SaveResult<T> Create<T>(SaveResult result) => result switch
		{
			SaveResult<T> srt => srt,
			ISaveResult<T> isrt => new(result, isrt.Value),
#warning this is certainly not the right way to do this
			_ => new(result, ((dynamic)result).Value),
		};

		public void Release()
		{
			while (ReleaseActions.Count > 0)
			{
				ReleaseActions.Dequeue()();
			}
		}
	}

	public class SaveResult<T> : SaveResult, ISaveResult<T>
	{
		public SaveResult(T value) : base(true) => Value = value;

		public SaveResult(SaveResult result, T value) : this(result.IsSuccess, result.ReleaseActions, value)
		{
		}

		public SaveResult(bool success, T value = default) : base(success) => Value = value;

		private SaveResult(bool success, Queue<Action> releaseActions, T value) : base(success, releaseActions) => Value = value;

		public T Value { get; }

		public static SaveResult<T> operator &(SaveResult<T> result, bool b) => new(result.IsSuccess && b, result.ReleaseActions, result.Value);
	}
}