using System;

namespace Utils.Properties
{
	public class ChangeProperty<T> : ChangePropertyBase<T>
		where T : IEquatable<T>
	{
		public ChangeProperty()
		{
		}

		public ChangeProperty(T fieldValue) : base(fieldValue)
		{
		}

		protected override bool AreEqual(T oldValue, T newValue) => SafeProperty.Equals(oldValue, newValue);
	}

	public class ChangeProperty<T1, T2> : ChangePropertyBase<T1, T2>
	where T1 : IEquatable<T1>
	{
		public ChangeProperty()
		{ }

		public ChangeProperty(T1 fieldValue) : base(fieldValue)
		{
		}

		protected override bool AreEqual(T1 oldValue, T1 newValue) => SafeProperty.Equals(oldValue, newValue);
	}
}