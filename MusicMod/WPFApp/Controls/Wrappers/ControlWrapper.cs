using System;
using System.Windows;

namespace WPFApp.Controls.Wrappers
{
	public interface IWrapper
	{
		FrameworkElement UIElement { get; }
	}

	public interface IReadableControlWrapper : IWrapper
	{
		bool TryGetValue(out object value);

		void ForceGetValue(out object value);
	}

	internal interface IReadableControlWrapper<TValue> : IWrapper
	{
		bool TryGetValue(out TValue value);

		void ForceGetValue(out TValue value);
	}

	internal interface IControlWrapper : IReadableControlWrapper
	{
		void SetValue(object value);
	}

	internal interface IControlWrapper<TValue> : IReadableControlWrapper<TValue>
	{
		void SetValue(TValue value);
	}

	internal abstract class ReadableControlWrapper<TValue, TControl> : IReadableControlWrapper, IReadableControlWrapper<TValue>
		where TControl : FrameworkElement
	{
		public abstract TControl UIElement { get; }

		FrameworkElement IWrapper.UIElement => UIElement;

		public void ForceGetValue(out object value)
		{
			ForceGetValue(out TValue x);
			value = x;
		}

		public void ForceGetValue(out TValue value)
		{
			if (!tryGetValue(out value))
			{
				throw new NotSupportedException();
			}
		}

		public bool TryGetValue(out TValue value)
		{
			bool result = tryGetValue(out TValue t) && Validate(t);
			SetStatus(result);
			value = t;
			return result;
		}

		public bool TryGetValue(out object value)
		{
			bool result = TryGetValue(out TValue t);
			value = t;
			return result;
		}

		protected abstract bool tryGetValue(out TValue value);

		protected virtual void SetStatus(bool? status) => SetStatus(status ?? true);

		protected virtual void SetStatus(bool status)
		{ }

		protected virtual bool Validate(TValue value) => true;
	}

	internal abstract class ControlWrapper<TValue, TControl> : ReadableControlWrapper<TValue, TControl>, IControlWrapper, IControlWrapper<TValue>
		where TControl : FrameworkElement
	{
		public void SetValue(TValue value)
		{
			setValue(value);
			SetStatus(null);
		}

		public void SetValue(object value)
		{
			var typed = (TValue)value;
			SetValue(typed);
		}

		protected abstract void setValue(TValue value);
	}
}