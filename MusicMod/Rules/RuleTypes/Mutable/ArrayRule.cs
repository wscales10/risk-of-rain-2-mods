using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rules.RuleTypes.Mutable
{
    public static class ArrayRule
    { }

    public class ArrayRule<TContext, TOut> : UpperRule<TContext, TOut>, IArrayRule<TContext, TOut>
    {
        public ArrayRule(params Rule<TContext, TOut>[] rules) : this((IEnumerable<Rule<TContext, TOut>>)rules)
        {
        }

        public ArrayRule(IEnumerable<Rule<TContext, TOut>> rules) => Rules = rules.ToList();

        public List<Rule<TContext, TOut>> Rules { get; }

        public override IEnumerable<(string, Rule<TContext, TOut>)> Children => Rules.SelectMany(r => r.Children);

        IEnumerable<IRule<TContext, TOut>> IArrayRule<TContext, TOut>.Rules => Rules;

        public override IEnumerable<Rule<TContext, TOut>> GetRules(TContext c) => Rules;

        public override IReadOnlyRule<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser) => new ReadOnlyArrayRule<TContext, TOut>(this, ruleParser);

        public override XElement ToXml()
        {
            var element = base.ToXml();
            element.Add(Rules.Select(b => b.ToXml()).ToArray());
            return element;
        }
    }
}