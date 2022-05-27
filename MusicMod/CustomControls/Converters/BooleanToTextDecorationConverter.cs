using System.Windows;
using System.Windows.Data;

namespace CustomControls.Converters
{
    [ValueConversion(typeof(bool), typeof(TextDecorationCollection))]
    public class BooleanToTextDecorationConverter : BooleanConverter<TextDecorationCollection>
    {
        public BooleanToTextDecorationConverter(TextDecorationCollection trueValue, TextDecorationCollection falseValue) : base(trueValue, falseValue)
        { }

        public BooleanToTextDecorationConverter() : this(TextDecorations.Underline, new TextDecorationCollection())
        {
        }
    }
}