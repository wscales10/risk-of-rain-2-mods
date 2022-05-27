using System;
using System.Globalization;
using System.Windows.Data;

namespace CustomControls.Converters
{
    public class CoalesceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
                object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return null;
            }

            foreach (object item in values)
            {
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
                object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}