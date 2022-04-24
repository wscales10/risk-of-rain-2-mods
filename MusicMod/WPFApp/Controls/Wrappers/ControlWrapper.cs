using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

		void Focus();
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
		private bool wantsFocus;

		private bool isInitialised;

		public abstract TControl UIElement { get; }

		FrameworkElement IWrapper.UIElement => UIElement;

		public virtual UIElement FocusElement => UIElement;

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
			bool? result = tryGetValue(out TValue t) && Validate(t);
			SetStatus(result);
			value = t;
			return result ?? false;
		}

		public bool TryGetValue(out object value)
		{
			bool result = TryGetValue(out TValue t);
			value = t;
			return result;
		}

		public void Focus()
		{
			if (!isInitialised)
			{
				FocusElement.IsVisibleChanged += (s, e) =>
				{
					if ((bool)e.NewValue && wantsFocus)
					{
						_ = ((UIElement)s).Focus();
						wantsFocus = false;
					}
				};

				isInitialised = true;
			}

			wantsFocus = true;
		}

		protected static void Outline(Control control, bool status)
		{
			control.BorderBrush = status switch
			{
				false => Brushes.Red,
				_ => Brushes.DarkGray,
			};
		}

		protected static void Outline(Control control, bool? status, bool threeWay = false)
		{
			control.BorderBrush = status switch
			{
				true when threeWay => Brushes.Green,
				false => Brushes.Red,
				_ => Brushes.DarkGray,
			};
		}

		protected abstract bool tryGetValue(out TValue value);

		protected virtual void SetStatus(bool? status) => SetStatus(status ?? true);

		protected virtual void SetStatus(bool status)
		{
			if (UIElement is Control c)
			{
				Outline(c, (bool?)status);
			}
		}

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
			if (value is not null and not TValue)
			{
				Debugger.Break();
			}

			var typed = (TValue)value;
			SetValue(typed);
		}

		protected abstract void setValue(TValue value);
	}
}