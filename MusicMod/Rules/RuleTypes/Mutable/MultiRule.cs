using Patterns.Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Utils.Reflection.Properties;

namespace Rules.RuleTypes.Mutable
{
    public class MultiRule<T, TContext> : UpperRule<TContext>, IMultiRule<T, TContext>
    {
        private readonly PatternGenerator<T> patternGenerator;

        public MultiRule(string propertyName, PatternGenerator<T> patternGenerator, Func<Rule<TContext>> ruleCreator, params T[] candidates)
        {
            PropertyName = propertyName;
            this.patternGenerator = patternGenerator;
            Pairs = candidates.Select(x => (x, ruleCreator())).ToList();
        }

        public List<(T expectedValue, Rule<TContext> rule)> Pairs { get; }

        public string PropertyName { get; }

        IEnumerable<(T expectedValue, IRule<TContext> rule)> IMultiRule<T, TContext>.Pairs => Pairs.Cast<(T, IRule<TContext>)>();

        public static explicit operator ArrayRule<TContext>(MultiRule<T, TContext> mr)
        {
            return (ArrayRule<TContext>)new ArrayRule<TContext>(mr.Pairs.Select((pair) => new IfRule<TContext>(PropertyPattern<TContext>.Create(mr.PropertyName, mr.patternGenerator(pair.expectedValue)), pair.rule)).ToArray()).Named(mr.Name);
        }

        public static explicit operator StaticSwitchRule<TContext>(MultiRule<T, TContext> mr)
        {
            return (StaticSwitchRule<TContext>)new StaticSwitchRule<TContext>(new PropertyInfo(mr.PropertyName, typeof(T)), null, mr.Pairs.Select(p => new RuleCase<TContext>(p.rule, mr.patternGenerator(p.expectedValue))).ToArray()).Named(mr.Name);
        }

        public override IEnumerable<Rule<TContext>> GetRules(TContext c)
        {
            T seenValue = c.GetPropertyValue<T>(PropertyName);

            foreach (var (expectedValue, rule) in Pairs)
            {
                if (patternGenerator(expectedValue).IsMatch(seenValue))
                {
                    yield return rule;
                }
            }

            yield return null;
        }

        public override XElement ToXml() => ((StaticSwitchRule<TContext>)this).ToXml();

        public override IReadOnlyRule<TContext> ToReadOnly() => new ReadOnlyMultiRule<T, TContext>(this);
    }
}