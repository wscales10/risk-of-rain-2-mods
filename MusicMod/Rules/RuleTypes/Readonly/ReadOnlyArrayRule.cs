using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyArrayRule<TContext, TOut> : ReadOnlyUpperRule<ArrayRule<TContext, TOut>, TContext, TOut>, IArrayRule<TContext, TOut>
    {
        public ReadOnlyArrayRule(ArrayRule<TContext, TOut> arrayRule) : base(arrayRule)
        {
            Rules = arrayRule.Rules.Select(r => r.ToReadOnly()).ToReadOnlyCollection();
        }

        public ReadOnlyCollection<IReadOnlyRule<TContext, TOut>> Rules { get; }

        IEnumerable<IRule<TContext, TOut>> IArrayRule<TContext, TOut>.Rules => Rules;
    }
}