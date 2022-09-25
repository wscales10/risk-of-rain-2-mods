using Rules.RuleTypes.Interfaces;

namespace Ror2Mod2
{
    public interface IRulePicker<TContext, TOut>
    {
        IRule<TContext, TOut> Rule { get; }
    }

    public class SingleRulePicker<TContext, TOut> : IRulePicker<TContext, TOut>
    {
        public SingleRulePicker(IRule<TContext, TOut> rule) => Rule = rule;

        public IRule<TContext, TOut> Rule { get; }
    }

    public class MutableRulePicker<TContext, TOut> : IRulePicker<TContext, TOut>
    {
        public IRule<TContext, TOut> Rule { get; set; }
    }
}