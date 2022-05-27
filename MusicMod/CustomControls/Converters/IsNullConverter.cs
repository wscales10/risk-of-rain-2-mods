using System.Windows.Data;

namespace CustomControls.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class IsNullConverter : SimpleValueConverter<object, bool>
    {
        protected override bool Convert(object value) => value is null;

        protected override object ConvertBack(bool value) => value ? base.ConvertBack(value) : null;
    }
}