using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BooleanToVisibilityConverter : IValueConverter
	{
		private readonly bool collapse;

		public BooleanToVisibilityConverter(bool collapse) => this.collapse = collapse;

		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (targetType != typeof(Visibility))
			{
				throw new InvalidOperationException("The target must be a Visibility");
			}

			if ((bool)value)
			{
				return Visibility.Visible;
			}
			else
			{
				return collapse ? Visibility.Collapsed : (object)Visibility.Hidden;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) => throw new NotSupportedException();
	}
}