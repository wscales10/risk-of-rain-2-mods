using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(bool), typeof(Brush))]
	public class BooleanToBrushConverter : IValueConverter
	{
		private readonly Func<bool, Brush> func;

		public BooleanToBrushConverter(Func<bool, Brush> func) => this.func = func;

		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (!targetType.IsAssignableFrom(typeof(Brush)))
			{
				throw new InvalidOperationException("The target must be a Brush");
			}

			return func((bool)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) => throw new NotSupportedException();
	}
}