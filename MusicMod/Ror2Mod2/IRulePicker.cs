using Rules.RuleTypes.Interfaces;

namespace Ror2Mod2
{
    public interface IRulePicker
    {
        IRule Rule { get; }
    }

    public class SingleRulePicker : IRulePicker
    {
        public SingleRulePicker(IRule rule) => Rule = rule;

        public IRule Rule { get; }
    }

    public class MutableRulePicker : IRulePicker
    {
        public IRule Rule { get; set; }
    }
}