using Patterns.Patterns.SmallPatterns.ValuePatterns;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Utils;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class EnumPatternWrapper<T> : RangePatternWrapper<EnumRangePattern<T>>
		where T : struct, Enum
	{
		private readonly ComboBox minComboBox;

		private readonly ComboBox maxComboBox;

		public EnumPatternWrapper(EnumRangePattern<T> pattern) : this()
		{
			setValue(pattern);
		}

		public EnumPatternWrapper()
		{
			minComboBox = new() { ItemsSource = GetItems(), DisplayMemberPath = nameof(Named<T>.Name), SelectedValuePath = nameof(Named<T>.Value) };
			minComboBox.SetResourceReference(FrameworkElement.StyleProperty, "ComboBoxStyle1");
			maxComboBox = new() { ItemsSource = GetItems(), DisplayMemberPath = nameof(Named<T>.Name), SelectedValuePath = nameof(Named<T>.Value) };
			maxComboBox.SetResourceReference(FrameworkElement.StyleProperty, "ComboBoxStyle1");
			UIElement.minContentPresenter.Content = minComboBox;
			UIElement.maxContentPresenter.Content = maxComboBox;
		}

		protected override bool Validate(EnumRangePattern<T> value)
		{
			return value.MinValue is not T min || value.MaxValue is not T max || EnumRangePattern<T>.GetIndex(max) >= EnumRangePattern.GetIndex(min);
		}

		protected override SaveResult<EnumRangePattern<T>> tryGetValue(GetValueRequest request)
		{
			return new SaveResult<EnumRangePattern<T>>(EnumRangePattern<T>.Create((T?)minComboBox.SelectedValue, (T?)maxComboBox.SelectedValue));
		}

		protected override void setValue(EnumRangePattern<T> value)
		{
			minComboBox.SelectedValue = value?.MinValue;
			maxComboBox.SelectedValue = value?.MaxValue;
		}

		private static ReadOnlyCollection<Named<T?>> GetItems() => new[] { new Named<T?>("(none)", null) }.Concat(Enum.GetValues<T>().Select(x => new Named<T?>($"{x} ({EnumRangePattern<T>.GetIndex(x)})", x))).ToReadOnlyCollection();
	}
}