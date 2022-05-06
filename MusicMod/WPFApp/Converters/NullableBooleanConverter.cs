using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace WPFApp.Converters
{
	public class NullableBooleanConverter<T> : IValueConverter
	{
		public NullableBooleanConverter(T trueValue, T falseValue, T nullValue)
		{
			True = trueValue;
			False = falseValue;
			Null = nullValue;
		}

		public T True { get; set; }

		public T False { get; set; }

		public T Null { get; set; }

		public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (value as bool?) switch
			{
				true => True,
				false => False,
				_ => Null,
			};
		}

		public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is T x && EqualityComparer<T>.Default.Equals(x, True);
	}
}