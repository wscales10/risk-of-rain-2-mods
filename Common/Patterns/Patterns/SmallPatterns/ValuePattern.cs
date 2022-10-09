using System;

namespace Patterns.Patterns.SmallPatterns
{
    public abstract class ValuePattern<T> : SmallPattern<T>, IValuePattern
    {
        private string definition;

        public bool Redefinable { get; } = true;

        public override sealed string Definition => definition;

        public override bool IsMatch(T value) => !(Definition is null) && isMatch(value);

        public ValuePattern<T> DefineWith(string stringDefinition)
        {
            if (stringDefinition is null)
            {
                throw new ArgumentNullException(nameof(stringDefinition));
            }

            if (Redefinable || Definition is null)
            {
                if (defineWith(stringDefinition))
                {
                    definition = stringDefinition;
                }
            }

            return this;
        }

        IValuePattern IValuePattern.DefineWith(string stringDefinition) => DefineWith(stringDefinition);

        protected abstract bool isMatch(T value);

        protected abstract bool defineWith(string stringDefinition);
    }

    public abstract class StructValuePattern<T> : ValuePattern<T>, IPattern<T?> where T : struct
    {
        public override sealed bool IsMatch(T value) => base.IsMatch(value);

        public bool IsMatch(T? value) => IsMatch(value.Value);

        IPattern<T?> IPattern<T?>.Simplify() => this;
    }

    public abstract class ClassValuePattern<T> : ValuePattern<T> where T : class
    {
        public override sealed bool IsMatch(T value) => !(value is null) && base.IsMatch(value);
    }
}