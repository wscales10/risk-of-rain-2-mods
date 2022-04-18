using System;

namespace Utils
{
	public delegate void PropertySetEventHandler<T>(T oldValue, T newValue);

	public delegate void PropertySetEventHandler<T1, T2>(T1 oldValue, T1 newValue, T2 extraParam);

	public delegate bool PropertyTrySetEventHandler<T>(T oldValue, T newValue);

	public delegate bool PropertyTrySetEventHandler<T1, T2>(T1 oldValue, T1 newValue, T2 extraParam);

	public delegate void PropertyChangedEventHandler<T>(T newValue);

	public delegate void PropertyChangedEventHandler<T1, T2>(T1 newValue, T2 extraParam);

	internal static class SafeProperty
	{
		internal static bool AmbiguousEquals<T>(T value1, T value2)
		{
			if (typeof(IEquatable<T>).IsAssignableFrom(typeof(T)))
			{
				return (value1 as IEquatable<T>) == (value2 as IEquatable<T>);
			}
			else
			{
				return object.Equals(value1, value2);
			}
		}

		internal static bool Equals<T>(IEquatable<T> value1, IEquatable<T> value2)
		{
			return value1?.Equals(value2) ?? value2 == null;
		}

		internal static bool Equals<T>(T value1, T value2)
			where T : IEquatable<T>
		{
			return value1?.Equals(value2) ?? value2 == null;
		}

		internal static bool Equals<T>(T? nullable1, T? nullable2)
			where T : struct, IEquatable<T>
		{
			return nullable1 is T value1 ? nullable2 is T value2 && value1.Equals(value2) : nullable2 is null;
		}
	}

	public abstract class SafePropertyBase<T> : IEquatable<SafePropertyBase<T>>
	{
		protected T fieldValue;

		protected SafePropertyBase(T fieldValue)
		{
			this.fieldValue = fieldValue;
		}

		public T Get() => fieldValue;

		public static implicit operator T(SafePropertyBase<T> prop) => prop.Get();

		public override string ToString() => fieldValue.ToString();

		public override bool Equals(object o) => Equals(o as SafePropertyBase<T>);

		public bool Equals(SafePropertyBase<T> p)
		{
			if (p is null)
			{
				return false;
			}

			if (ReferenceEquals(this, p))
			{
				return true;
			}

			if (GetType() != p.GetType())
			{
				return false;
			}

			return SafeProperty.AmbiguousEquals(fieldValue, p.fieldValue);
		}

		public override int GetHashCode() => fieldValue.GetHashCode();

		public static bool operator ==(SafePropertyBase<T> p1, SafePropertyBase<T> p2) => p1 is null ? p2 is null : p1.Equals(p2);

		public static bool operator !=(SafePropertyBase<T> p1, SafePropertyBase<T> p2) => !(p1 == p2);
	}

	public class SetProperty<T> : SafePropertyBase<T>
	{
		public SetProperty(T fieldValue = default) : base(fieldValue)
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
		public SetProperty(T1 fieldValue = default) : base(fieldValue)
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

	public abstract class ChangePropertyBase<T> : SetProperty<T>
	{
		protected ChangePropertyBase(T fieldValue) : base(fieldValue)
		{
		}

		public event PropertyChangedEventHandler<T> OnChanged;

		protected abstract bool AreEqual(T oldValue, T newValue);

		public override T Set(T value)
		{
			T oldValue = base.Set(value);

			if (!AreEqual(oldValue, value))
			{
				OnChanged?.Invoke(value);
			}

			return oldValue;
		}
	}

	public abstract class ChangePropertyBase<T1, T2> : SetProperty<T1, T2>
	{
		protected ChangePropertyBase(T1 fieldValue) : base(fieldValue)
		{
		}

		public event PropertyChangedEventHandler<T1, T2> OnChanged;

		protected abstract bool AreEqual(T1 oldValue, T1 newValue);

		public override T1 Set(T1 value, T2 extraParam)
		{
			T1 oldValue = base.Set(value, extraParam);

			if (!AreEqual(oldValue, value))
			{
				OnChanged?.Invoke(value, extraParam);
			}

			return oldValue;
		}
	}

	public class ChangeProperty<T> : ChangePropertyBase<T>
		where T : IEquatable<T>
	{
		public ChangeProperty(T fieldValue = default) : base(fieldValue)
		{
		}

		protected override bool AreEqual(T oldValue, T newValue) => SafeProperty.Equals(oldValue, newValue);
	}

	public class ChangeProperty<T1, T2> : ChangePropertyBase<T1, T2>
		where T1 : IEquatable<T1>
	{
		public ChangeProperty(T1 fieldValue = default) : base(fieldValue)
		{
		}

		protected override bool AreEqual(T1 oldValue, T1 newValue) => SafeProperty.Equals(oldValue, newValue);
	}

	public class NullableChangeProperty<T> : ChangePropertyBase<T?>
		where T : struct, IEquatable<T>
	{
		public NullableChangeProperty(T? fieldValue = null) : base(fieldValue)
		{
		}

		protected override bool AreEqual(T? oldValue, T? newValue) => SafeProperty.Equals(oldValue, newValue);
	}

	public class NullableChangeProperty<T1, T2> : ChangePropertyBase<T1?, T2>
		where T1 : struct, IEquatable<T1>
	{
		protected NullableChangeProperty(T1? fieldValue = null) : base(fieldValue)
		{
		}

		protected override bool AreEqual(T1? oldValue, T1? newValue) => SafeProperty.Equals(oldValue, newValue);
	}
}
