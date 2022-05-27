using System;

namespace CustomControls.Converters
{
    public abstract class SimpleValueConverter<TIn, TOut> : ValueConverter<TIn, TOut>
    {
        protected sealed override TOut Convert(TIn value, object parameter) => Convert(value);

        protected abstract TOut Convert(TIn value);

        protected sealed override TIn ConvertBack(TOut value, object parameter) => ConvertBack(value);

        protected virtual TIn ConvertBack(TOut value) => throw new NotSupportedException();
    }
}