using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyMultiRule<T, TContext> : ReadOnlyRule<MultiRule<T, TContext>, TContext>
    {
        public ReadOnlyMultiRule(MultiRule<T, TContext> multiRule) : base(multiRule)
        {
            PropertyName = multiRule.PropertyName;
            Pairs = multiRule.Pairs.Select(p => (p.expectedValue, p.rule.ToReadOnly())).ToReadOnlyCollection();
        }

        public ReadOnlyCollection<(T expectedValue, IReadOnlyRule<TContext> rule)> Pairs { get; }

        public string PropertyName { get; }
    }
}