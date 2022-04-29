using MyRoR2;
using Patterns;
using Rules.RuleTypes.Interfaces;
using Spotify.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Utils;

namespace Rules.RuleTypes.Mutable
{
	public delegate Pattern<T> PatternGenerator<T>(T input);

	public abstract class Rule : IRule, ITreeItem<Rule>, ITreeItem
	{
		public string Name
		{
			get;
			set;
		}

		public virtual IEnumerable<(string, Rule)> Children => Enumerable.Empty<(string, Rule)>();

		IEnumerable<(string, ITreeItem)> ITreeItem.Children => Children.Select(p => (p.Item1, (ITreeItem)p.Item2));

		public static Rule FromXml(XElement element)
		{
			if (element is null)
			{
				return null;
			}

			var name = element.Attribute("name")?.Value;
			return GetUnnamed().Named(name);

			Rule GetUnnamed()
			{
				switch (element.Attribute("type").Value)
				{
					case nameof(ArrayRule):
						return new ArrayRule(element.Elements(nameof(Rule)).Select(FromXml).ToArray());

					case nameof(IfRule):
						var ifElement = element.Elements().First();
						return new IfRule(RoR2PatternParser.Instance.Parse<Context>(ifElement), FromXml(element.Element("Then").OnlyChild()), FromXml(element.Element("Else")?.OnlyChild()));

					case nameof(Bucket):
						return new Bucket(element.Elements().Select(Command.FromXml).ToList());

					case nameof(StaticSwitchRule):
						return StaticSwitchRule.Parse(element);

					default:
						throw new XmlException();
				}
			}
		}

		public static implicit operator Rule(Command c) => new Bucket(c);

		public static implicit operator Rule(CommandList commands) => new Bucket(commands);

		public static Rule Create(Type ruleType)
		{
			switch (ruleType.Name)
			{
				case nameof(IfRule):
					return new IfRule();

				case nameof(StaticSwitchRule):
					return new StaticSwitchRule();

				case nameof(ArrayRule):
					return new ArrayRule();

				case nameof(Bucket):
					return new Bucket();

				default:
					return null;
			}
		}

		public abstract Bucket GetBucket(Context c);

		IBucket IRule.GetBucket(Context c) => GetBucket(c);

		public CommandList GetCommands(Context oldContext, Context newContext, bool force = false)
		{
			var newBucket = GetBucket(newContext);
			if (!(newBucket?.Commands is null))
			{
				foreach (var command in newBucket.Commands)
				{
					this.Log($"{command?.GetType()}");
				}
			}

#warning implement force on error
			if (newBucket is null || force || GetBucket(oldContext) != newBucket)
			{
				return newBucket?.Commands;
			}
			else
			{
				return null;
			}
		}

		ICommandList IRule.GetCommands(Context oldContext, Context newContext, bool force)
		{
			return GetCommands(oldContext, newContext, force);
		}

		public abstract IReadOnlyRule ToReadOnly();

		public override string ToString() => Name ?? GetType().Name;

		public virtual XElement ToXml()
		{
			var element = new XElement(nameof(Rule));
			element.SetAttributeValue("type", GetType().Name);

			if (!(Name is null))
			{
				element.SetAttributeValue("name", Name);
			}

			return element;
		}

		internal Rule Named(string name)
		{
			Name = name;
			return this;
		}
	}
}