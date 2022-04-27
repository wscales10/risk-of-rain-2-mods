using Patterns.Patterns.SmallPatterns;
using System.Windows.Controls;
using System.Windows.Data;
using WPFApp.Converters;
using System.Windows;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal static class NullPatternWrapper
	{
		internal static BooleanToStringConverter Converter { get; } = new(b => b ? "is null" : "is not null");
	}

	internal class ClassNullPatternWrapper<T> : PatternWrapper<ClassNullPattern<T>, CheckBox>
		where T : class
	{
		protected ClassNullPatternWrapper(ClassNullPattern<T> pattern)
		{
			Binding binding = new(nameof(UIElement.IsChecked))
			{
				Source = UIElement,
				Converter = NullPatternWrapper.Converter
			};

			_ = UIElement.SetBinding(ContentControl.ContentProperty, binding);
			SetValue(pattern ?? ClassNullPattern<T>.IsNull);
			UIElement.Checked += (s, e) => NotifyValueChanged();
			UIElement.Unchecked += (s, e) => NotifyValueChanged();
		}

		public override CheckBox UIElement { get; } = new() { HorizontalAlignment = HorizontalAlignment.Center };

		protected override void setValue(ClassNullPattern<T> value) => UIElement.IsChecked = value.IsMatch(null);

		protected override SaveResult<ClassNullPattern<T>> tryGetValue(bool trySave) => new((bool)UIElement.IsChecked ? ClassNullPattern<T>.IsNull : ClassNullPattern<T>.IsNotNull);
	}

	internal class NullableNullPatternWrapper<T> : PatternWrapper<NullableNullPattern<T>, CheckBox>
		where T : struct
	{
		protected NullableNullPatternWrapper(NullableNullPattern<T> pattern)
		{
			Binding binding = new(nameof(UIElement.IsChecked))
			{
				Source = UIElement,
				Converter = NullPatternWrapper.Converter
			};

			_ = UIElement.SetBinding(ContentControl.ContentProperty, binding);
			SetValue(pattern ?? NullableNullPattern<T>.IsNull);
			UIElement.Checked += (s, e) => NotifyValueChanged();
			UIElement.Unchecked += (s, e) => NotifyValueChanged();
		}

		public override CheckBox UIElement { get; } = new() { HorizontalAlignment = HorizontalAlignment.Center };

		protected override void setValue(NullableNullPattern<T> value) => UIElement.IsChecked = value.IsMatch(null);

		protected override SaveResult<NullableNullPattern<T>> tryGetValue(bool trySave) => new((bool)UIElement.IsChecked ? NullableNullPattern<T>.IsNull : NullableNullPattern<T>.IsNotNull);
	}
}