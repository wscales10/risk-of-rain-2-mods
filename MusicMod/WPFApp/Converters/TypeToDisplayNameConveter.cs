using System;
using System.Globalization;
using System.Windows.Data;
using Utils;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(Type), typeof(string))]
	public class TypeToDisplayNameConverter : IValueConverter
	{
		private readonly bool withGenericTypeArguments;

		public TypeToDisplayNameConverter(bool withGenericTypeArguments) => this.withGenericTypeArguments = withGenericTypeArguments;

		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (targetType != typeof(string))
			{
				throw new InvalidOperationException("The target must be a string");
			}

			return ((Type)value).GetDisplayName(withGenericTypeArguments);
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) => throw new NotSupportedException();
	}
}
