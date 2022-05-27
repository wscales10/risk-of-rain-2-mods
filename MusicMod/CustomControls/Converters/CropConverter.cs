using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CustomControls.Converters
{
    public class CropConverter : IMultiValueConverter
    {
        public float Width { get; set; } = 1;

        public float Height { get; set; } = 1;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(ImageSource))
            {
                throw new InvalidOperationException("The target must be a CroppedBitmap");
            }

            var bmp = (BitmapImage)values[0];

            if (bmp is null)
            {
                return null;
            }

            if (values.Contains(DependencyProperty.UnsetValue))
            {
                return bmp;
            }

            double actualWidth = (double)values[1];
            double actualHeight = (double)values[2];
            double targetHeight = actualWidth * Height / Width;

            Int32Rect rect = new(0, (int)((actualHeight - targetHeight) / 2), (int)actualWidth, (int)targetHeight);

            return new CroppedBitmap(bmp, rect);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}