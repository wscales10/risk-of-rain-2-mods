using System;
using System.Globalization;
using System.Windows.Data;
using Utils;

namespace WPFApp.Converters
{
    public abstract class ValueConverter<TIn, TOut> : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CheckType(targetType, typeof(TOut));
            return Convert((TIn)value, parameter);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CheckType(targetType, typeof(TIn));
            return ConvertBack((TOut)value, parameter);
        }

        protected static void CheckType(Type targetType, Type desiredType)
        {
            if (!targetType.IsAssignableFrom(desiredType))
            {
                throw new InvalidOperationException($"The target must be a {desiredType.GetDisplayName()}");
            }
        }

        protected abstract TOut Convert(TIn value, object parameter);

        protected virtual TIn ConvertBack(TOut value, object parameter) => throw new NotSupportedException();
    }
}