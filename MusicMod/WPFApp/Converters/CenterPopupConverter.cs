using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace WPFApp.Converters
{
	public class CenterPopupConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values.Contains(DependencyProperty.UnsetValue))
			{
				return double.NaN;
			}

			double placementTargetWidth = (double)values[0];
			double popupWidth = (double)values[1];
			return (placementTargetWidth / 2.0) - (popupWidth / 2.0);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
	}
}