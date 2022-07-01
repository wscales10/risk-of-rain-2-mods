using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Utils;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyCase<TValue, TContext> : ICase<TValue, TContext>
    {
        private readonly string toString;

        public ReadOnlyCase(RuleCase<TValue, TContext> mutable)
        {
            toString = mutable.ToString();
            Name = mutable.Name;
            WherePattern = new ReadOnlyPattern<TContext>(mutable.WherePattern);
            Arr = mutable.Arr.ToReadOnlyCollection();
            Output = mutable.Output.ToReadOnly();
        }

        public ReadOnlyPattern<TContext> WherePattern { get; }

        public ReadOnlyCollection<TValue> Arr { get; }

        public IRule<TContext> Output { get; }

        public string Name { get; }

        IPattern<TContext> ICase<TValue, TContext>.WherePattern => WherePattern;

        IEnumerable<TValue> ICase<TValue, TContext>.Arr => Arr;

        public override string ToString() => toString;
    }
}