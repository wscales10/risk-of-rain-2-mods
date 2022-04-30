using System;
using System.Globalization;
using System.Windows.Data;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(Type), typeof(bool))]
	public class TypeToBooleanConverter : IValueConverter
	{
		public Type Type { get; set; }

		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (targetType != typeof(bool))
			{
				throw new InvalidOperationException("The target must be a boolean");
			}

			return Type.IsAssignableFrom(value?.GetType());
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) => throw new NotSupportedException();
	}
}