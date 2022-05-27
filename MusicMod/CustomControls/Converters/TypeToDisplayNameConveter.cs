using System;
using System.Windows.Data;
using Utils;

namespace CustomControls.Converters
{
    [ValueConversion(typeof(Type), typeof(string))]
    public class TypeToDisplayNameConverter : SimpleValueConverter<Type, string>
    {
        private readonly bool withGenericTypeArguments;

        public TypeToDisplayNameConverter(bool withGenericTypeArguments) => this.withGenericTypeArguments = withGenericTypeArguments;

        protected override string Convert(Type value) => value.GetDisplayName(withGenericTypeArguments);
    }
}