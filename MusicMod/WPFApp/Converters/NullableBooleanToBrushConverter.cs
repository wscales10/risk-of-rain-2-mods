using System.Windows.Data;
using System.Windows.Media;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(bool?), typeof(Brush))]
	public class NullableBooleanToBrushConverter : NullableBooleanConverter<Brush>
	{
		public NullableBooleanToBrushConverter(Brush trueValue, Brush falseValue, Brush nullValue) : base(trueValue, falseValue, nullValue)
		{ }

		public NullableBooleanToBrushConverter() : this(Brushes.Green, Brushes.Red, Brushes.DarkGray)
		{
		}
	}
}