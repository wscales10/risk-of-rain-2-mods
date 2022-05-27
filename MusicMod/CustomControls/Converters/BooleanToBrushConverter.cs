using System.Windows.Data;
using System.Windows.Media;

namespace CustomControls.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BooleanToBrushConverter : BooleanConverter<Brush>
    {
        public BooleanToBrushConverter(Brush trueValue, Brush falseValue) : base(trueValue, falseValue)
        { }

        public BooleanToBrushConverter() : this(Brushes.DarkGray, Brushes.Red)
        {
        }
    }
}