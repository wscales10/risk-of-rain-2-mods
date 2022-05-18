using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPFApp.Converters
{
    [ValueConversion(typeof(HorizontalAlignment), typeof(GridLength))]
    public class AlignmentToGridLengthConverter : IValueConverter
    {
        public GridLength Left { get; set; }

        public GridLength Right { get; set; }

        public GridLength Center { get; set; }

        public GridLength Stretch { get; set; }

        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (!targetType.IsAssignableFrom(typeof(GridLength)))
            {
                throw new InvalidOperationException("The target must be a GridLength");
            }

            return (HorizontalAlignment)value switch
            {
                HorizontalAlignment.Left => Left,
                HorizontalAlignment.Center => Center,
                HorizontalAlignment.Right => Right,
                HorizontalAlignment.Stretch => Stretch,
                _ => throw new ArgumentOutOfRangeException(nameof(value)),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture) => throw new NotSupportedException();
    }
}