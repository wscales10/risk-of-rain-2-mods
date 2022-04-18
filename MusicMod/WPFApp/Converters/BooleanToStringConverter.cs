using System;
using System.Globalization;
using System.Windows.Data;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(bool), typeof(string))]
	public class BooleanToStringConverter : IValueConverter
	{
		private readonly Func<bool, string> func;

		public BooleanToStringConverter(Func<bool, string> func) => this.func = func;

		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (!targetType.IsAssignableFrom(typeof(string)))
			{
				throw new InvalidOperationException("The target must be a string");
			}

			return func((bool)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) => throw new NotSupportedException();
	}
}