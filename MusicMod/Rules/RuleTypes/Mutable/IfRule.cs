using MyRoR2;
using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Rules.RuleTypes.Mutable
{
    public class IfRule : UpperRule, IIfRule
    {
        public IfRule(IPattern<Context> pattern = null, Rule thenRule = null, Rule elseRule = null)
        {
            Pattern = pattern;
            ThenRule = thenRule;
            ElseRule = elseRule;
        }

        public IPattern<Context> Pattern { get; set; }

        public Rule ThenRule { get; set; }

        public Rule ElseRule { get; set; }

        IRule IIfRule.ThenRule => ThenRule;

        IRule IIfRule.ElseRule => ElseRule;

        public override IEnumerable<(string, Rule)> Children => new[] { (Pattern.ToString(), ThenRule), ("Otherwise", ElseRule) };

        public override Rule GetRule(Context c) => Pattern.IsMatch(c) ? ThenRule : ElseRule;

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

        public override IReadOnlyRule ToReadOnly() => new ReadOnlyIfRule(this);
    }
}