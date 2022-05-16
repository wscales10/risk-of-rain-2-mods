using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace WPFApp.Converters
{
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }

        public T False { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b && b ? True : False;

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is T x && EqualityComparer<T>.Default.Equals(x, True);
    }
}