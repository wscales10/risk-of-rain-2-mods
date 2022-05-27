using System.Windows;
using System.Windows.Data;

namespace CustomControls.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter(Visibility trueValue, Visibility falseValue) : base(trueValue, falseValue)
        { }

        public BooleanToVisibilityConverter() : this(Visibility.Visible, Visibility.Collapsed)
        {
        }
    }
}