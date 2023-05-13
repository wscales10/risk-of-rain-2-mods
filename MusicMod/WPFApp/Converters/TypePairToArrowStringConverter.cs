using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Utils;

namespace WPFApp.Converters
{
	[ValueConversion(typeof((Type, Type)), typeof(string))]
	public class TypePairToArrowStringConverter : IValueConverter
	{
		private static readonly Dictionary<Type, string> customTypeNames = new();

		static TypePairToArrowStringConverter()
		{
			customTypeNames.Add(typeof(MyRoR2.RoR2Context), "Risk of Rain 2");
			customTypeNames.Add(typeof(string), "String");
			customTypeNames.Add(typeof(Spotify.Commands.ICommandList), "Spotify");
		}

		public TypePairToArrowStringConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter,
			CultureInfo culture)
		{
			if (value is null)
			{
				return string.Empty;
			}

			if (!targetType.IsAssignableFrom(typeof(string)))
			{
				throw new InvalidOperationException("The target must be a string");
			}

			var (t1, t2) = ((Type, Type))value;

			return $"{GetString(t1)} -> {GetString(t2)}";
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) => throw new NotSupportedException();

		private static string GetString(Type type)
		{
			if (customTypeNames.TryGetValue(type, out var output))
			{
				return output;
			}

			return type.GetDisplayName();
		}
	}
}