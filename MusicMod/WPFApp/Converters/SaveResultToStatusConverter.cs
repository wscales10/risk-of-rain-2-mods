using System;
using System.Globalization;
using System.Windows.Data;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Converters
{
    [ValueConversion(typeof(SaveResult), typeof(bool?))]
    public class SaveResultToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (targetType != typeof(bool?))
            {
                throw new InvalidOperationException("The target must be a nullable boolean");
            }

            return (value as SaveResult)?.Status;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture) => throw new NotSupportedException();
    }
}