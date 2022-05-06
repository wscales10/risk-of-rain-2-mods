using System;
using System.Globalization;
using System.Windows.Data;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(double), typeof(double))]
	public class RatioConverter : IValueConverter
	{
		public float Antecedent { get; set; } = 1;

		public float Consequent { get; set; } = 1;

		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (targetType != typeof(double))
			{
				throw new InvalidOperationException("The target must be a double");
			}

			return (double)value * Consequent / Antecedent;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (targetType != typeof(double))
			{
				throw new InvalidOperationException("The target must be a double");
			}

			return (double)value * Antecedent / Consequent;
		}
	}
}