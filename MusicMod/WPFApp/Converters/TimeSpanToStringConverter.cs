using System;
using System.Windows.Data;
using Utils;

namespace WPFApp.Converters
{
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    public class TimeSpanToCompactStringConverter : SimpleValueConverter<TimeSpan, string>
    {
        protected override string Convert(TimeSpan value) => value.ToCompactString();

        protected override TimeSpan ConvertBack(string value)
        {
            return TimeSpanMethods.TryParseCompactString(value, out TimeSpan timeSpan) ? timeSpan : throw new FormatException();
        }
    }
}