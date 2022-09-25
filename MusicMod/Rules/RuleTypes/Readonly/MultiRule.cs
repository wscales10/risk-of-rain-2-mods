using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyMultiRule<T, TContext, TOut> : ReadOnlyUpperRule<MultiRule<T, TContext, TOut>, TContext, TOut>
    {
        public ReadOnlyMultiRule(MultiRule<T, TContext, TOut> multiRule) : base(multiRule)
        {
            PropertyName = multiRule.PropertyName;
            Pairs = multiRule.Pairs.Select(p => (p.expectedValue, p.rule.ToReadOnly())).ToReadOnlyCollection();
        }

        public ReadOnlyCollection<(T expectedValue, IReadOnlyRule<TContext, TOut> rule)> Pairs { get; }

        public string PropertyName { get; }
    }
}