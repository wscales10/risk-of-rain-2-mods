namespace Utils.Properties
{
	public abstract class ChangePropertyBase<T> : SetProperty<T>
	{
		protected ChangePropertyBase()
		{ }

		protected ChangePropertyBase(T fieldValue) : base(fieldValue)
		{
		}

		public event PropertyChangedEventHandler<T> OnChanged;

		public override T Set(T value)
		{
			T oldValue = base.Set(value);

			if (!AreEqual(oldValue, value))
			{
				OnChanged?.Invoke(value);
			}

			return oldValue;
		}

		protected abstract bool AreEqual(T oldValue, T newValue);
	}

	public abstract class ChangePropertyBase<T1, T2> : SetProperty<T1, T2>
	{
		protected ChangePropertyBase()
		{ }

		protected ChangePropertyBase(T1 fieldValue) : base(fieldValue)
		{
		}

		public event PropertyChangedEventHandler<T1, T2> OnChanged;

		public override T1 Set(T1 value, T2 extraParam)
		{
			T1 oldValue = base.Set(value, extraParam);

			if (!AreEqual(oldValue, value))
			{
				OnChanged?.Invoke(value, extraParam);
			}

			return oldValue;
		}

		protected abstract bool AreEqual(T1 oldValue, T1 newValue);
	}
}