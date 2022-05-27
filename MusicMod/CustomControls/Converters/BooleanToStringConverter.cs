using System;
using System.Windows.Data;

namespace CustomControls.Converters
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToStringConverter : SimpleValueConverter<bool, string>
    {
        private readonly Func<bool, string> func;

        public BooleanToStringConverter(Func<bool, string> func) => this.func = func;

        protected override string Convert(bool value) => func(value);
    }
}