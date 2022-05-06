using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WPFApp.Converters
{
	public class CropConverter2 : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (!targetType.IsAssignableFrom(typeof(CroppedBitmap)))
			{
				throw new InvalidOperationException("The target must be a CroppedBitmap");
			}

			var bmp = (BitmapImage)values[0];
			var rect = (Int32Rect)values[1];

			return bmp is null ? null : new CroppedBitmap(bmp, rect);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			if (value is null)
			{
				throw new NotSupportedException();
			}
			else
			{
				var bmp = (CroppedBitmap)value;
				return new object[] { bmp.Source, bmp.SourceRect };
			}
		}
	}
}