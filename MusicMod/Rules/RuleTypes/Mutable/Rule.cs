﻿using Patterns;
using Rules.RuleTypes.Interfaces;
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

	public static class Rule
	{
	}

	public abstract class RuleBase : IXmlExportable
	{
		public string Name
		{
			get;
			set;
		}

		public override string ToString() => Name ?? GetType().GetDisplayName();

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

	public abstract class Rule<TContext, TOut> : RuleBase, IRule<TContext, TOut>, ITreeItem<Rule<TContext, TOut>>, ITreeItem
	{
		public virtual IEnumerable<(string, Rule<TContext, TOut>)> Children => Enumerable.Empty<(string, Rule<TContext, TOut>)>();

		IEnumerable<(string, ITreeItem)> ITreeItem.Children => Children.Select(p => (p.Item1, (ITreeItem)p.Item2));

		public static implicit operator Rule<TContext, TOut>(TOut output) => new Bucket<TContext, TOut>(output);

		public static Rule<TContext, TOut> Create(Type ruleType) => (Rule<TContext, TOut>)ruleType.MakeGenericType(typeof(TContext), typeof(TOut)).ConstructDefault();

		public abstract TrackedResponse<TContext, TOut> GetBucket(TContext c);

		public abstract IReadOnlyRule<TContext, TOut> ToReadOnly(RuleParser<TContext, TOut> ruleParser);

		public TOut GetOutput(TContext newContext) => GetOutput(this, newContext);

		public Rule<TContext, TOut> Named(string name)
		{
			Name = name;
			return this;
		}

		public override XElement ToXml()
		{
			var element = base.ToXml();

			element.SetAttributeValue(nameof(TContext), typeof(TContext).FullName);
			element.SetAttributeValue(nameof(TOut), typeof(TOut).FullName);

			return element;
		}

		internal static TOut GetOutput(IRule<TContext, TOut> rule, TContext newContext)
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
			else
			{
				rule.Log($"{nameof(newBucket)} is not null");
				return newBucket.Output;
			}
		}
	}
}