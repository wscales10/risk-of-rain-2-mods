using System;
using System.Xml.Linq;

namespace Patterns
{
    public class ReadOnlyPattern : IPattern
    {
        private readonly IPattern pattern;

        public ReadOnlyPattern(IPattern pattern) => this.pattern = pattern;

        public Type ValueType => pattern.ValueType;

        public IPattern Correct() => new ReadOnlyPattern(pattern.Correct());

        public bool IsMatch(object obj) => pattern.IsMatch(obj);

        public XElement ToXml() => pattern.ToXml();
    }

    public class ReadOnlyPattern<T> : IPattern<T>
    {
        private readonly IPattern<T> pattern;

        public ReadOnlyPattern(IPattern<T> pattern) => this.pattern = pattern;

        public Type ValueType => pattern.ValueType;

        public IPattern Correct() => new ReadOnlyPattern(pattern.Correct());

        public bool IsMatch(object obj) => pattern.IsMatch(obj);

        public bool IsMatch(T value) => pattern.IsMatch(value);

        public IPattern<T> Simplify() => new ReadOnlyPattern<T>(pattern.Simplify());

        public XElement ToXml() => pattern.ToXml();
    }
}