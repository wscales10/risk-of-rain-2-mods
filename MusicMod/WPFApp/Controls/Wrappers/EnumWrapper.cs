using System;
using System.Windows.Controls;
using Utils.Reflection;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    internal static class EnumWrapper
	{
		public static IControlWrapper Create(Type type) => (IControlWrapper)typeof(EnumWrapper<>).MakeGenericType(type).Construct();
	}

	internal class EnumWrapper<T> : ControlWrapper<T, ComboBox>
		where T : struct, Enum
	{
		public EnumWrapper() => UIElement.SelectionChanged += (s, e) => NotifyValueChanged();

		public override ComboBox UIElement { get; } = new ComboBox { ItemsSource = Enum.GetValues<T>(), IsTextSearchEnabled = true };

		public override string ValueString => Utils.HelperMethods.GetNullSafeString(UIElement.SelectedItem);

		protected override void setValue(T value) => UIElement.SelectedItem = value;

		protected override SaveResult<T> tryGetValue(bool trySave) => UIElement.SelectedItem is not T x ? (new(false)) : (new(x));
	}
}