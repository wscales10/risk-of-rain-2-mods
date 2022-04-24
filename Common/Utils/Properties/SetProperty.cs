using System;

namespace Utils.Properties
{
	public class SetProperty<T> : SafePropertyBase<T>
	{
		public SetProperty()
		{ }

		public SetProperty(T fieldValue) : base(fieldValue)
		{
		}

		public event PropertySetEventHandler<T> OnSet;

		public event PropertyTrySetEventHandler<T> OnTrySet;

		public virtual T Set(T value)
		{
			T oldValue = fieldValue;

			if (!(OnTrySet?.Invoke(oldValue, value) ?? true))
			{
				throw new InvalidOperationException();
			}

			fieldValue = value;
			OnSet?.Invoke(oldValue, value);
			return oldValue;
		}
	}

	public class SetProperty<T1, T2> : SafePropertyBase<T1>
	{
		public SetProperty()
		{ }

		public SetProperty(T1 fieldValue) : base(fieldValue)
		{
		}

		public event PropertySetEventHandler<T1, T2> OnSet;

		public event PropertyTrySetEventHandler<T1, T2> OnTrySet;

		public virtual T1 Set(T1 value, T2 extraParam)
		{
			T1 oldValue = fieldValue;

			if (!(OnTrySet?.Invoke(oldValue, value, extraParam) ?? true))
			{
				throw new InvalidOperationException();
			}

			fieldValue = value;
			OnSet?.Invoke(oldValue, value, extraParam);
			return oldValue;
		}
	}
}