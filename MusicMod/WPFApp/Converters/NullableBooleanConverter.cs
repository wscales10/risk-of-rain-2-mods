using System.Collections.Generic;

namespace WPFApp.Converters
{
    public class NullableBooleanConverter<T> : SimpleValueConverter<bool?, T>
    {
        public NullableBooleanConverter(T trueValue, T falseValue, T nullValue)
        {
            True = trueValue;
            False = falseValue;
            Null = nullValue;
        }

        public T True { get; set; }

        public T False { get; set; }

        public T Null { get; set; }

        protected override T Convert(bool? value) => value switch
        {
            true => True,
            false => False,
            _ => Null,
        };

        protected override bool? ConvertBack(T value)
        {
            if (EqualityComparer<T>.Default.Equals(value, True))
            {
                return true;
            }

            if (EqualityComparer<T>.Default.Equals(value, Null))
            {
                return null;
            }

            return false;
        }
    }
}