using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Utils.Coroutines
{
	public class Reference
	{
		public Reference(object parameter = null)
		{
			Parameter = parameter;
		}

		public object Parameter { get; }

		public object Value { get; private set; }

		public bool IsFinished
		{
			get
			{
				switch (Status)
				{
					case TaskStatus.Canceled:
					case TaskStatus.Faulted:
					case TaskStatus.RanToCompletion:
						return true;

					case TaskStatus.Created:
					case TaskStatus.Running:
						return false;

					default:
						throw new NotSupportedException($"{nameof(Status)} should not be {Status}");
				}
			}
		}

		public TaskStatus Status { get; private set; }

		public void Complete(object value = null)
		{
			if (Status <= TaskStatus.Running)
			{
				Value = value;
				Status = TaskStatus.RanToCompletion;
			}
			else
			{
				Throw();
			}
		}

		public void Fail()
		{
			if (Status < TaskStatus.RanToCompletion)
			{
				Value = null;
				Status = TaskStatus.Faulted;
			}
			else
			{
				Throw();
			}
		}

		public void Cancel()
		{
			if (Status < TaskStatus.RanToCompletion)
			{
				Value = null;
				Status = TaskStatus.Canceled;
			}
			else
			{
				Throw();
			}
		}

		private void Throw([CallerMemberName] string methodName = null)
		{
			throw new InvalidOperationException($"Cannot {methodName?.ToLower()} as status is {Status}");
		}
	}
}