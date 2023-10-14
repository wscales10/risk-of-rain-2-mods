using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Rules.RuleTypes.Mutable
{
    public static class ArrayRule
    {
        public static ArrayRule<TContext, TOut> Create<TContext, TOut>(params Rule<TContext, TOut>[] rules)
        {
            return new ArrayRule<TContext, TOut>(rules);
        }

        public static ArrayRule<TContext, TOut> Create<TContext, TOut>(IEnumerable<Rule<TContext, TOut>> rules)
        {
            return new ArrayRule<TContext, TOut>(rules);
        }

        internal static IEnumerable<(string, IRule<TContext, TOut>)> GetChildren<TContext, TOut>(IArrayRule<TContext, TOut> rule) => rule.Rules.SelectMany(r => r.Children);
    }

    public class ArrayRule<TContext, TOut> : UpperRule<TContext, TOut>, IArrayRule<TContext, TOut>
    {
        public ArrayRule(params Rule<TContext, TOut>[] rules) : this((IEnumerable<Rule<TContext, TOut>>)rules)
        {
        }

        public ArrayRule(IEnumerable<Rule<TContext, TOut>> rules) => Rules = rules.ToList();

        public bool IsRandom { get; set; }

        public List<Rule<TContext, TOut>> Rules { get; }

        public override IEnumerable<(string, IRule<TContext, TOut>)> Children => ArrayRule.GetChildren(this);

        IEnumerable<IRule<TContext, TOut>> IArrayRule<TContext, TOut>.Rules => Rules;

        public ArrayRule<TContext, TOut> MakeRandom(bool shouldBeRandom = true)
        {
            IsRandom = shouldBeRandom;
            return this;
        }

        public override IEnumerable<Rule<TContext, TOut>> GetRules(TContext c)
        {
            var random = new Random();

            if (IsRandom)
            {
                return Rules.OrderBy(r => random.NextDouble());
            }
            else
            {
                return Rules;
            }
        }

        public override IReadOnlyRule<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser) => new ReadOnlyArrayRule<TContext, TOut>(this, ruleParser);

        public override XElement ToXml()
        {
            var element = base.ToXml();
            element.SetAttributeValue(nameof(IsRandom), IsRandom);
            element.Add(Rules.Select(b => b.ToXml()).ToArray());
            return element;
        }
    }
}