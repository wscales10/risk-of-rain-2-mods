using Patterns;
using Rules.RuleTypes.Interfaces;
using Spotify.Commands;
using System;
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

    public abstract class Rule<TContext> : Rule, IRule<TContext>, ITreeItem<Rule<TContext>>, ITreeItem
    {
        public virtual IEnumerable<(string, Rule<TContext>)> Children => Enumerable.Empty<(string, Rule<TContext>)>();

        IEnumerable<(string, ITreeItem)> ITreeItem.Children => Children.Select(p => (p.Item1, (ITreeItem)p.Item2));

        public static implicit operator Rule<TContext>(Command c) => new Bucket<TContext>(c);

        public static implicit operator Rule<TContext>(CommandList commands) => new Bucket<TContext>(commands);

        public static Rule<TContext> Create(Type ruleType) => (Rule<TContext>)ruleType.ConstructDefault();

        public abstract TrackedResponse<TContext> GetBucket(TContext c);

        public ICommandList GetCommands(TContext oldContext, TContext newContext, bool force = false)
        {
            var newBucketResponse = GetBucket(newContext);
            var newBucket = newBucketResponse.Bucket;
            newBucketResponse.LogRules();

            if (!(newBucket?.Commands is null))
            {
                foreach (var command in newBucket.Commands)
                {
                    this.Log($"Command Type: {command?.GetType().GetDisplayName() ?? "null"}");
                }
            }

            if (newBucket is null)
            {
                this.Log($"{nameof(newBucket)} is null");
            }
            else

            // TODO: implement force on error
            if (force)
            {
                this.Log("forcing retry");
            }
            else if (GetBucket(oldContext).Bucket != newBucket)
            {
                this.Log($"{nameof(newBucket)} is different");
            }
            else
            {
                this.Log($"{nameof(newBucket)} is no different from before");
                return null;
            }

            return newBucket?.Commands;
        }

        public abstract IReadOnlyRule<TContext> ToReadOnly();

        internal Rule<TContext> Named(string name)
        {
            Name = name;
            return this;
        }
    }
}