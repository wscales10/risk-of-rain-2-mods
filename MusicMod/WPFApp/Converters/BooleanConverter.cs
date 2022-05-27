using System.Collections.Generic;

namespace WPFApp.Converters
{
    public class BooleanConverter<T> : SimpleValueConverter<bool, T>
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        public T True { get; set; }

        public T False { get; set; }

        protected override T Convert(bool value) => value ? True : False;

        protected override bool ConvertBack(T value) => EqualityComparer<T>.Default.Equals(value, True);
    }
}