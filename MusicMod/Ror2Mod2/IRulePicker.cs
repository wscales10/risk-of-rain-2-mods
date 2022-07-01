using Rules.RuleTypes.Interfaces;

namespace Ror2Mod2
{
    public interface IRulePicker<TContext>
    {
        IRule<TContext> Rule { get; }
    }

    public class SingleRulePicker<TContext> : IRulePicker<TContext>
    {
        public SingleRulePicker(IRule<TContext> rule) => Rule = rule;

        public IRule<TContext> Rule { get; }
    }

    public class MutableRulePicker<TContext> : IRulePicker<TContext>
    {
        public IRule<TContext> Rule { get; set; }
    }
}