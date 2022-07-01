using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rules.RuleTypes.Mutable
{
    public static class ArrayRule
    { }

    public class ArrayRule<TContext> : UpperRule<TContext>, IArrayRule<TContext>
    {
        public ArrayRule(params Rule<TContext>[] rules) : this((IEnumerable<Rule<TContext>>)rules)
        {
        }

        public ArrayRule(IEnumerable<Rule<TContext>> rules) => Rules = rules.ToList();

        public List<Rule<TContext>> Rules { get; }

        public override IEnumerable<(string, Rule<TContext>)> Children => Rules.SelectMany(r => r.Children);

        IEnumerable<IRule<TContext>> IArrayRule<TContext>.Rules => Rules;

        public override IEnumerable<Rule<TContext>> GetRules(TContext c) => Rules;

        public override IReadOnlyRule<TContext> ToReadOnly() => new ReadOnlyArrayRule<TContext>(this);

        public override XElement ToXml()
        {
            var element = base.ToXml();
            element.Add(Rules.Select(b => b.ToXml()).ToArray());
            return element;
        }
    }
}