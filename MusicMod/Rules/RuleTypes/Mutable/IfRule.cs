using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Rules.RuleTypes.Mutable
{
    public static class IfRule
    {
    }

    public class IfRule<TContext, TOut> : UpperRule<TContext, TOut>, IIfRule<TContext, TOut>
    {
        public IfRule(IPattern<TContext> pattern = null, Rule<TContext, TOut> thenRule = null, Rule<TContext, TOut> elseRule = null)
        {
            Pattern = pattern;
            ThenRule = thenRule;
            ElseRule = elseRule;
        }

        public IPattern<TContext> Pattern { get; set; }

        public Rule<TContext, TOut> ThenRule { get; set; }

        public Rule<TContext, TOut> ElseRule { get; set; }

        IRule<TContext, TOut> IIfRule<TContext, TOut>.ThenRule => ThenRule;

        IRule<TContext, TOut> IIfRule<TContext, TOut>.ElseRule => ElseRule;

        public override IEnumerable<(string, Rule<TContext, TOut>)> Children => new[] { (Pattern.ToString(), ThenRule), ("Otherwise", ElseRule) };

        public override IEnumerable<Rule<TContext, TOut>> GetRules(TContext c)
        {
            yield return Pattern.IsMatch(c) ? ThenRule : ElseRule;
        }

        public override XElement ToXml()
        {
            var element = base.ToXml();

            if (!(Pattern is null))
            {
                var ifElement = Pattern.Simplify().ToXml();
                element.Add(ifElement);
            }

            if (!(ThenRule is null))
            {
                var thenElement = new XElement("Then", ThenRule.ToXml());
                element.Add(thenElement);
            }

            if (!(ElseRule is null))
            {
                var elseElement = new XElement("Else", ElseRule.ToXml());
                element.Add(elseElement);
            }

            return element;
        }

        public override IReadOnlyRule<TContext, TOut> ToReadOnly() => new ReadOnlyIfRule<TContext, TOut>(this);
    }
}