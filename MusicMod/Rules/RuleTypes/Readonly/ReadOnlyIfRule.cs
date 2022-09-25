using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;

namespace Rules.RuleTypes.Readonly
{
    public class ReadOnlyIfRule<TContext, TOut> : ReadOnlyUpperRule<IfRule<TContext, TOut>, TContext, TOut>, IIfRule<TContext, TOut>
    {
        public ReadOnlyIfRule(IfRule<TContext, TOut> ifRule) : base(ifRule)
        {
            Pattern = new ReadOnlyPattern<TContext>(ifRule.Pattern);
            ThenRule = ifRule.ThenRule.ToReadOnly();
            ElseRule = ifRule.ElseRule?.ToReadOnly();
        }

        public IPattern<TContext> Pattern { get; }

        public IRule<TContext, TOut> ThenRule { get; }

        public IRule<TContext, TOut> ElseRule { get; }
    }
}