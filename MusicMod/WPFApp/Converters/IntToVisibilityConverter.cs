using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(int), typeof(Visibility))]
	public class IntToVisibilityConverter : IValueConverter
	{
		private readonly Predicate<int> predicate;

		public IntToVisibilityConverter(Predicate<int> predicate) => this.predicate = predicate;

		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (targetType != typeof(Visibility))
			{
				throw new InvalidOperationException("The target must be a Visibility");
			}

			return predicate((int)value) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) => throw new NotSupportedException();
	}
}
