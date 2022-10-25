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
	public class MultiRule<T, TContext, TOut> : UpperRule<TContext, TOut>, IMultiRule<T, TContext, TOut>
	{
		private readonly PatternGenerator<T> patternGenerator;

		public MultiRule(string propertyName, PatternGenerator<T> patternGenerator, Func<Rule<TContext, TOut>> ruleCreator, params T[] candidates)
		{
			PropertyName = propertyName;
			this.patternGenerator = patternGenerator;
			Pairs = candidates.Select(x => (x, ruleCreator())).ToList();
		}

		public List<(T expectedValue, Rule<TContext, TOut> rule)> Pairs { get; }

		public string PropertyName { get; }

		IEnumerable<(T expectedValue, IRule<TContext, TOut> rule)> IMultiRule<T, TContext, TOut>.Pairs => Pairs.Cast<(T, IRule<TContext, TOut>)>();

		public static explicit operator ArrayRule<TContext, TOut>(MultiRule<T, TContext, TOut> mr)
		{
			return (ArrayRule<TContext, TOut>)new ArrayRule<TContext, TOut>(mr.Pairs.Select((pair) => new IfRule<TContext, TOut>(Query<TContext>.Create(mr.PropertyName, mr.patternGenerator(pair.expectedValue)), pair.rule)).ToArray()).Named(mr.Name);
		}

		public static explicit operator StaticSwitchRule<TContext, TOut>(MultiRule<T, TContext, TOut> mr)
		{
			return (StaticSwitchRule<TContext, TOut>)new StaticSwitchRule<TContext, TOut>(new PropertyInfo(mr.PropertyName, typeof(T)), null, mr.Pairs.Select(p => new RuleCase<TContext, TOut>(p.rule, mr.patternGenerator(p.expectedValue))).ToArray()).Named(mr.Name);
		}

		public override IEnumerable<Rule<TContext, TOut>> GetRules(TContext c)
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

		public override XElement ToXml() => ((StaticSwitchRule<TContext, TOut>)this).ToXml();

		public override IReadOnlyRule<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser) => new ReadOnlyMultiRule<T, TContext, TOut>(this, ruleParser);
	}
}