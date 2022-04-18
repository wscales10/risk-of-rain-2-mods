using System;
using System.Windows.Data;
using System.Globalization;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(bool), typeof(bool))]
	public class InverseBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture) => targetType == typeof(bool) ? !(bool)value : throw new InvalidOperationException("The target must be a boolean");

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) => throw new NotSupportedException();
	}
}