using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyArrayRule<TContext> : ReadOnlyRule<ArrayRule<TContext>, TContext>, IArrayRule<TContext>
    {
        public ReadOnlyArrayRule(ArrayRule<TContext> arrayRule) : base(arrayRule)
        {
            Rules = arrayRule.Rules.Select(r => r.ToReadOnly()).ToReadOnlyCollection();
        }

        public ReadOnlyCollection<IReadOnlyRule<TContext>> Rules { get; }

        IEnumerable<IRule<TContext>> IArrayRule<TContext>.Rules => Rules;
    }
}