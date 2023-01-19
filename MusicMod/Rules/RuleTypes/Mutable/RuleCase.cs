using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.RuleTypes.Mutable
{
	public static class MultiCase
	{
		public static MultiCase<TValue, TContext, TOut> Create<TValue, TContext, TOut>(Func<TValue, Rule<TContext, TOut>> ruleGenerator, params TValue[] candidates)
		{
			return new MultiCase<TValue, TContext, TOut>(ruleGenerator, candidates);
		}
	}

	public class RuleCase<TContext, TOut> : RuleCase<IPattern, TContext, TOut>
	{
		public RuleCase(Rule<TContext, TOut> rule, params IPattern[] arr) : base(rule, arr)
		{
		}

		public RuleCase(Rule<TContext, TOut> rule, IPattern<TContext> wherePattern, params IPattern[] arr) : base(rule, wherePattern, arr)
		{
		}

		public static RuleCase<T, TContext, TOut> C<T>(Rule<TContext, TOut> rule, params T[] arr) => new RuleCase<T, TContext, TOut>(rule, arr);

		public static RuleCase<T, TContext, TOut> C<T>(Rule<TContext, TOut> rule, Pattern<TContext> wherePattern, params T[] arr) => new RuleCase<T, TContext, TOut>(rule, wherePattern, arr);

		public RuleCase<TContext, TOut> DeepClone(RuleParser<TContext, TOut> ruleParser) => new RuleCase<TContext, TOut>(ruleParser.DeepClone(Output), WherePattern is null ? null : ruleParser.PatternParser.DeepClone(WherePattern), Arr.Select(ruleParser.PatternParser.DeepClone).ToArray());
	}

	public class RuleCase<TValue, TContext, TOut> : Case<TValue, Rule<TContext, TOut>>, ICase<TValue, TContext, TOut>, ICaseGetter<TValue, TContext, TOut>
	{
		public RuleCase(Rule<TContext, TOut> rule, params TValue[] arr) : base(rule, arr)
		{
		}

		public RuleCase(Rule<TContext, TOut> rule, IPattern<TContext> wherePattern, params TValue[] arr) : this(rule, arr)
		{
			WherePattern = wherePattern;
		}

		public IPattern<TContext> WherePattern { get; set; }

		IEnumerable<TValue> ICase<TValue, TContext, TOut>.Arr => Arr;

		IRule<TContext, TOut> ICase<TValue, TContext, TOut>.Output => Output;

		public string Name { get; set; }

		public static implicit operator RuleCase<TValue, TContext, TOut>((TValue, Rule<TContext, TOut>) pair)
		{
			return new RuleCase<TValue, TContext, TOut>(pair.Item2, pair.Item1);
		}

		public static implicit operator RuleCase<TValue, TContext, TOut>((TValue, IPattern<TContext>, Rule<TContext, TOut>) triplet)
		{
			return new RuleCase<TValue, TContext, TOut>(triplet.Item3, triplet.Item2, triplet.Item1);
		}

		public override string ToString()
		{
			if (!(Name is null))
			{
				return Name;
			}

			if (WherePattern is null)
			{
				return base.ToString();
			}
			else
			{
				return $"{base.ToString()} with {WherePattern}";
			}
		}

		public ReadOnlyCase<TValue, TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser) => new ReadOnlyCase<TValue, TContext, TOut>(this, ruleParser);

		public IEnumerable<RuleCase<TValue, TContext, TOut>> GetCases()
		{
			yield return this;
		}

		internal RuleCase<TValue, TContext, TOut> Named(string name)
		{
			Name = name;
			return this;
		}
	}

	public class MultiCase<TValue, TContext, TOut> : ICaseGetter<TValue, TContext, TOut>
	{
		private readonly Func<TValue, Rule<TContext, TOut>> ruleGenerator;

		private readonly TValue[] candidates;

		public MultiCase(Func<TValue, Rule<TContext, TOut>> ruleGenerator, params TValue[] candidates)
		{
			this.ruleGenerator = ruleGenerator;
			this.candidates = candidates;
		}

		public IEnumerable<RuleCase<TValue, TContext, TOut>> GetCases() => candidates.Select(c => new RuleCase<TValue, TContext, TOut>(ruleGenerator(c), c));
	}
}