using System;
using System.Globalization;
using System.Windows.Data;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(object), typeof(bool))]
	public class IsNullConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (targetType != typeof(bool))
			{
				throw new InvalidOperationException("The target must be a boolean");
			}

			return value is null;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (value is not bool b)
			{
				throw new InvalidOperationException("The value must be a boolean");
			}

			if (b)
			{
				throw new NotSupportedException();
			}
			else
			{
				return null;
			}
		}
	}
}