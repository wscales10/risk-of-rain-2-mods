using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyIfRule<TContext> : ReadOnlyRule<IfRule<TContext>, TContext>, IIfRule<TContext>
    {
        public ReadOnlyIfRule(IfRule<TContext> ifRule) : base(ifRule)
        {
            Pattern = new ReadOnlyPattern<TContext>(ifRule.Pattern);
            ThenRule = ifRule.ThenRule.ToReadOnly();
            ElseRule = ifRule.ElseRule?.ToReadOnly();
        }

        public IPattern<TContext> Pattern { get; }

        public IRule<TContext> ThenRule { get; }

        public IRule<TContext> ElseRule { get; }
    }
}