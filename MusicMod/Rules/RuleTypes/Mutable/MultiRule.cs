using MyRoR2;
using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Utils;
using Utils.Reflection.Properties;

namespace Rules.RuleTypes.Mutable
{
    public class MultiRule<T> : UpperRule, IMultiRule<T>
    {
        private readonly PatternGenerator<T> patternGenerator;

        public MultiRule(string propertyName, PatternGenerator<T> patternGenerator, Func<Rule> ruleCreator, params T[] candidates)
        {
            PropertyName = propertyName;
            this.patternGenerator = patternGenerator;
            Pairs = candidates.Select(x => (x, ruleCreator())).ToList();
        }

        public List<(T expectedValue, Rule rule)> Pairs { get; }

        public string PropertyName { get; }

        IEnumerable<(T expectedValue, IRule rule)> IMultiRule<T>.Pairs => Pairs.Cast<(T, IRule)>();

        public static explicit operator ArrayRule(MultiRule<T> mr)
        {
            return (ArrayRule)new ArrayRule(mr.Pairs.Select((pair) => new IfRule(Query.Create(mr.PropertyName, mr.patternGenerator(pair.expectedValue)), pair.rule)).ToArray()).Named(mr.Name);
        }

        public static explicit operator StaticSwitchRule(MultiRule<T> mr)
        {
            return (StaticSwitchRule)new StaticSwitchRule(new PropertyInfo(mr.PropertyName, typeof(T)), null, mr.Pairs.Select(p => new Case(p.rule, mr.patternGenerator(p.expectedValue))).ToArray()).Named(mr.Name);
        }

        public override IEnumerable<Rule> GetRules(Context c)
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

        public override XElement ToXml() => ((StaticSwitchRule)this).ToXml();

        public override IReadOnlyRule ToReadOnly() => new ReadOnlyMultiRule<T>(this);
    }
}