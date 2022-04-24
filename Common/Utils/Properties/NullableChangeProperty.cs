using System;

namespace Utils.Properties
{
	public class NullableChangeProperty<T> : ChangePropertyBase<T?>
		where T : struct, IEquatable<T>
	{
		public NullableChangeProperty()
		{ }

		public NullableChangeProperty(T? fieldValue) : base(fieldValue)
		{
		}

		protected override bool AreEqual(T? oldValue, T? newValue) => SafeProperty.Equals(oldValue, newValue);
	}

	public class NullableChangeProperty<T1, T2> : ChangePropertyBase<T1?, T2>
	where T1 : struct, IEquatable<T1>
	{
		public NullableChangeProperty()
		{ }

		public NullableChangeProperty(T1? fieldValue) : base(fieldValue)
		{
		}

		protected override bool AreEqual(T1? oldValue, T1? newValue) => SafeProperty.Equals(oldValue, newValue);
	}
}