using System;
using System.Windows.Controls;
using Utils.Reflection;

namespace WPFApp.Controls.Wrappers
{
	internal static class EnumWrapper
	{
		public static IControlWrapper Create(Type type) => (IControlWrapper)typeof(EnumWrapper<>).MakeGenericType(type).Construct();
	}

	internal class EnumWrapper<T> : ControlWrapper<T, ComboBox>
		where T : struct, Enum
	{
		public override ComboBox UIElement { get; } = new ComboBox { ItemsSource = Enum.GetValues<T>(), IsTextSearchEnabled = true };

		protected override void setValue(T value) => UIElement.SelectedItem = value;

		protected override bool tryGetValue(out T value)
		{
			if (UIElement.SelectedItem is not T x)
			{
				value = default;
				return false;
			}

			value = x;
			return true;
		}
	}
}