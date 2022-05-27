using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CustomControls.Converters
{
    public class FlagsToMarginsConverter : IMultiValueConverter
    {
        public Thickness Minimum { get; set; }

        public Thickness Maximum { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!targetType.IsAssignableFrom(typeof(Thickness)))
            {
                throw new InvalidOperationException("The target must be a Thickness");
            }

            var needsLeftMargin = values[0] is bool b1 && b1;
            var needsRightMargin = values[1] is bool b2 && b2;

            return new Thickness((needsLeftMargin ? Maximum : Minimum).Left, Maximum.Top, (needsRightMargin ? Maximum : Minimum).Right, Maximum.Bottom);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}