using Patterns;
using Rules.RuleTypes.Interfaces;
using Spotify.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Utils;
using Utils.Reflection;

namespace Rules.RuleTypes.Mutable
{
    public delegate Pattern<T> PatternGenerator<T>(T input);

    public abstract class Rule : IXmlExportable
    {
        public string Name
        {
            get;
            set;
        }

        public override string ToString() => Name ?? GetType().Name;

        public virtual XElement ToXml()
        {
            var element = new XElement(nameof(Rule));
            element.SetAttributeValue("type", GetType().GetDisplayName(false));

            if (!(Name is null))
            {
                element.SetAttributeValue("name", Name);
            }

            return element;
        }
    }

    public abstract class Rule<TContext, TOut> : Rule, IRule<TContext, TOut>, ITreeItem<Rule<TContext, TOut>>, ITreeItem
    {
        public virtual IEnumerable<(string, Rule<TContext, TOut>)> Children => Enumerable.Empty<(string, Rule<TContext, TOut>)>();

        IEnumerable<(string, ITreeItem)> ITreeItem.Children => Children.Select(p => (p.Item1, (ITreeItem)p.Item2));

        public static implicit operator Rule<TContext, TOut>(TOut output) => new Bucket<TContext, TOut>(output);

        public static Rule<TContext, TOut> Create(Type ruleType) => (Rule<TContext, TOut>)ruleType.MakeGenericType(typeof(TContext), typeof(TOut)).ConstructDefault();

        public abstract TrackedResponse<TContext, TOut> GetBucket(TContext c);

        public abstract IReadOnlyRule<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser);

        public TOut GetCommands(TContext oldContext, TContext newContext, bool force = false) => GetCommands(this, oldContext, newContext, force);

        internal static TOut GetCommands(IRule<TContext, TOut> rule, TContext oldContext, TContext newContext, bool force = false)
        {
            var newBucketResponse = rule.GetBucket(newContext);
            var newBucket = newBucketResponse.Bucket;
            newBucketResponse.LogRules();

            if (newBucket is IEnumerable collection)
            {
                foreach (var item in collection)
                {
                    rule.Log($"Item Type: {item?.GetType().GetDisplayName() ?? "null"}");
                }
            }

            if (newBucket is null)
            {
                rule.Log($"{nameof(newBucket)} is null");
                return default;
            }
            else if (force)
            {
                // TODO: implement force on error
                rule.Log("forcing retry");
            }
            else if (rule.GetBucket(oldContext).Bucket != newBucket)
            {
                rule.Log($"{nameof(newBucket)} is different");
            }
            else
            {
                rule.Log($"{nameof(newBucket)} is no different from before");
                return default;
            }

            return newBucket.Output;
        }

        internal Rule<TContext, TOut> Named(string name)
        {
            Name = name;
            return this;
        }
    }
}