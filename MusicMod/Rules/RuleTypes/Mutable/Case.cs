using MyRoR2;
using Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules.RuleTypes.Mutable
{
    public class Case : Case<IPattern>
    {
        public Case(Rule rule, params IPattern[] arr) : base(rule, arr)
        {
        }

        public Case(Rule rule, IPattern<Context> wherePattern, params IPattern[] arr) : base(rule, wherePattern, arr)
        {
        }

        public static Case<T> C<T>(Rule rule, params T[] arr) => new Case<T>(rule, arr);

        public static Case<T> C<T>(Rule rule, Pattern<Context> wherePattern, params T[] arr) => new Case<T>(rule, wherePattern, arr);

        public Case DeepClone(PatternParser patternParser) => new Case(Output.DeepClone(), WherePattern is null ? null : patternParser.DeepClone(WherePattern), Arr.Select(patternParser.DeepClone).ToArray());
    }

    public class Case<TValue> : Case<TValue, Rule>, ICase<TValue>, ICaseGetter<TValue>
    {
        public Case(Rule rule, params TValue[] arr) : base(rule, arr)
        {
        }

        public Case(Rule rule, IPattern<Context> wherePattern, params TValue[] arr) : this(rule, arr)
        {
            WherePattern = wherePattern;
        }

        public IPattern<Context> WherePattern { get; set; }

        IEnumerable<TValue> ICase<TValue>.Arr => Arr;

        IRule ICase<TValue>.Output => Output;

        public string Name { get; set; }

        public static implicit operator Case<TValue>((TValue, Rule) pair)
        {
            return new Case<TValue>(pair.Item2, pair.Item1);
        }

        public static implicit operator Case<TValue>((TValue, IPattern<Context>, Rule) triplet)
        {
            return new Case<TValue>(triplet.Item3, triplet.Item2, triplet.Item1);
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

        public ReadOnlyCase<TValue> ToReadOnly() => new ReadOnlyCase<TValue>(this);

        public IEnumerable<Case<TValue>> GetCases()
        {
            yield return this;
        }

        internal Case<TValue> Named(string name)
        {
            Name = name;
            return this;
        }
    }

    public class MultiCase<TValue> : ICaseGetter<TValue>
    {
        private readonly Func<Rule> ruleGenerator;

        private readonly TValue[] candidates;

        public MultiCase(Func<Rule> ruleGenerator, params TValue[] candidates)
        {
            this.ruleGenerator = ruleGenerator;
            this.candidates = candidates;
        }

        public IEnumerable<Case<TValue>> GetCases() => candidates.Select(c => new Case<TValue>(ruleGenerator(), c));
    }
}