using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.RuleTypes.Mutable
{
    public class RuleCase<TContext> : RuleCase<IPattern, TContext>
    {
        public RuleCase(Rule<TContext> rule, params IPattern[] arr) : base(rule, arr)
        {
        }

        public RuleCase(Rule<TContext> rule, IPattern<TContext> wherePattern, params IPattern[] arr) : base(rule, wherePattern, arr)
        {
        }

        public static RuleCase<T, TContext> C<T>(Rule<TContext> rule, params T[] arr) => new RuleCase<T, TContext>(rule, arr);

        public static RuleCase<T, TContext> C<T>(Rule<TContext> rule, Pattern<TContext> wherePattern, params T[] arr) => new RuleCase<T, TContext>(rule, wherePattern, arr);

        public RuleCase<TContext> DeepClone(RuleParser<TContext> ruleParser) => new RuleCase<TContext>(ruleParser.DeepClone(Output), WherePattern is null ? null : ruleParser.PatternParser.DeepClone(WherePattern), Arr.Select(ruleParser.PatternParser.DeepClone).ToArray());
    }

    public class RuleCase<TValue, TContext> : Case<TValue, Rule<TContext>>, ICase<TValue, TContext>, ICaseGetter<TValue, TContext>
    {
        public RuleCase(Rule<TContext> rule, params TValue[] arr) : base(rule, arr)
        {
        }

        public RuleCase(Rule<TContext> rule, IPattern<TContext> wherePattern, params TValue[] arr) : this(rule, arr)
        {
            WherePattern = wherePattern;
        }

        public IPattern<TContext> WherePattern { get; set; }

        IEnumerable<TValue> ICase<TValue, TContext>.Arr => Arr;

        IRule<TContext> ICase<TValue, TContext>.Output => Output;

        public string Name { get; set; }

        public static implicit operator RuleCase<TValue, TContext>((TValue, Rule<TContext>) pair)
        {
            return new RuleCase<TValue, TContext>(pair.Item2, pair.Item1);
        }

        public static implicit operator RuleCase<TValue, TContext>((TValue, IPattern<TContext>, Rule<TContext>) triplet)
        {
            return new RuleCase<TValue, TContext>(triplet.Item3, triplet.Item2, triplet.Item1);
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

        public ReadOnlyCase<TValue, TContext> ToReadOnly() => new ReadOnlyCase<TValue, TContext>(this);

        public IEnumerable<RuleCase<TValue, TContext>> GetCases()
        {
            yield return this;
        }

        internal RuleCase<TValue, TContext> Named(string name)
        {
            Name = name;
            return this;
        }
    }

    public class MultiCase<TValue, TContext> : ICaseGetter<TValue, TContext>
    {
        private readonly Func<Rule<TContext>> ruleGenerator;

        private readonly TValue[] candidates;

        public MultiCase(Func<Rule<TContext>> ruleGenerator, params TValue[] candidates)
        {
            this.ruleGenerator = ruleGenerator;
            this.candidates = candidates;
        }

        public IEnumerable<RuleCase<TValue, TContext>> GetCases() => candidates.Select(c => new RuleCase<TValue, TContext>(ruleGenerator(), c));
    }
}