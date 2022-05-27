using System.Windows.Data;

namespace CustomControls.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class RatioConverter : SimpleValueConverter<double, double>
    {
        public float Antecedent { get; set; } = 1;

        public float Consequent { get; set; } = 1;

        protected override double Convert(double value) => value * Consequent / Antecedent;

        protected override double ConvertBack(double value) => value * Antecedent / Consequent;
    }
}