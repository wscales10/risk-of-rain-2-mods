using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System;
using System.Linq;

namespace RuleExamples
{
    public static class Generate
    {
        public static Rule<string, TOut> FromCategoriser<TContext, TOut>(IRule<TContext, string> categoriser, Switcher<string, TOut> switcher, Func<TOut> outputGenerator)
        {
            var children = categoriser.Children.Where(c => c.Item2 is not null).ToArray();
            return FromChildren(children, switcher, outputGenerator);
        }

        private static Rule<string, TOut> FromChildren<TContext, TOut>((string name, IRule<TContext, string> rule)[] children, Switcher<string, TOut> switcher, Func<TOut> outputGenerator)
        {
            Bucket<string, TOut> newBucket() => new(outputGenerator());

            var grouped = children.GroupBy(c => c.rule is Bucket<TContext, string>);

            (string name, Bucket<TContext, string> rule)[] buckets = grouped.SingleOrDefault(g => g.Key)?.Select(p => (p.name, (Bucket<TContext, string>)p.rule)).ToArray() ?? Array.Empty<(string, Bucket<TContext, string>)>();
            var notBuckets = grouped.SingleOrDefault(g => !g.Key)?.ToArray() ?? Array.Empty<(string, IRule<TContext, string>)>();

            switch (buckets.Length)
            {
                case 0:
                    if (children.Length == 1)
                    {
                        return FromCategoriser(children[0].rule, switcher, outputGenerator).Named(children[0].name);
                    }
                    else
                    {
                        return new ArrayRule<string, TOut>(children.Select(p => FromCategoriser(p.rule, switcher, outputGenerator).Named(p.name)).ToArray());
                    }

                case 1:
                    if (children.Length == 1)
                    {
                        return newBucket().Named(children[0].name);
                    }
                    else
                    {
                        return IfRule.Create(StringPattern.Equals(buckets[0].rule.Output), newBucket().Named(buckets[0].name), FromChildren(notBuckets, switcher, outputGenerator));
                    }

                default:
                    var output = switcher.Create(buckets.Select(c => new RuleCase<string, string, TOut>(newBucket().Named(c.name), c.rule.Output)).ToArray());
                    output.DefaultRule = notBuckets.Length == 0 ? null : FromChildren(notBuckets, switcher, outputGenerator);
                    return output;
            }
        }
    }
}