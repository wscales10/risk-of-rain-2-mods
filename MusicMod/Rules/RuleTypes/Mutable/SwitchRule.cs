using MyRoR2;
using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Readonly;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Utils;
using Utils.Reflection.Properties;

namespace Rules.RuleTypes.Mutable
{
#warning could have an issue with enums?

	public class SwitchRule<T> : UpperRule
	{
		private readonly PatternGenerator<T> patternGenerator;

		public SwitchRule(string propertyName, params ICaseGetter<T>[] cases)
		{
			PropertyName = propertyName;
			Cases = cases.SelectMany(c => c.GetCases()).ToReadOnlyCollection();
		}

		public SwitchRule(string propertyName, PatternGenerator<T> patternGenerator, params ICaseGetter<T>[] cases) : this(propertyName, cases)
		{
			this.patternGenerator = patternGenerator;
		}

		public SwitchRule(string propertyName, Rule defaultRule, params ICaseGetter<T>[] cases) : this(propertyName, cases)
		{
			DefaultRule = defaultRule;
		}

		public SwitchRule(string propertyName, Rule defaultRule, PatternGenerator<T> patternGenerator, params ICaseGetter<T>[] cases) : this(propertyName, cases)
		{
			DefaultRule = defaultRule;
			this.patternGenerator = patternGenerator;
		}

		public string PropertyName { get; }

		public ReadOnlyCollection<Case<T>> Cases { get; }

		public Rule DefaultRule { get; }

		public static explicit operator ArrayRule(SwitchRule<T> sr)
		{
			var rules = sr.Cases.Select(c =>
			{
				var pattern = new OrPattern<T>(c.Arr.Select(sr.GeneratePattern).ToArray());
				return new IfRule(Query.Create(sr.PropertyName, pattern) & (c.WherePattern ?? ConstantPattern<Context>.True), c.Output);
			}).Cast<Rule>().ToList();

			if (!(sr.DefaultRule is null))
				rules.Add(sr.DefaultRule);

			return (ArrayRule)new ArrayRule(rules).Named(sr.Name);
		}

		public static explicit operator StaticSwitchRule(SwitchRule<T> sr)
		{
			return (StaticSwitchRule)new StaticSwitchRule(
				new PropertyInfo(sr.PropertyName, typeof(T)),
				sr.DefaultRule,
				sr.Cases.Select(c => new Case<IPattern>(c.Output, c.WherePattern, c.Arr.Select(sr.GeneratePattern).ToArray()).Named(c.Name)).ToArray()).Named(sr.Name);
		}

		public override Rule GetRule(Context c)
		{
			T seenValue = c.GetPropertyValue<T>(PropertyName);

			foreach (var @case in Cases)
			{
				if (@case.WherePattern?.IsMatch(c) != false)
				{
					if (patternGenerator is null ? @case.Arr.Contains(seenValue) : @case.Arr.Any(allowedValue => patternGenerator(allowedValue).IsMatch(seenValue)))
					{
						return @case.Output;
					}
				}
			}

			return DefaultRule;
		}

		public override XElement ToXml()
		{
			// return ((ArrayRule)this).ToXml();
			return ((StaticSwitchRule)this).ToXml();
		}

		public override IReadOnlyRule ToReadOnly() => ((StaticSwitchRule)this).ToReadOnly();

		private IPattern<T> GeneratePattern(T value)
		{
			return patternGenerator is null ? RoR2PatternParser.Instance.GetEqualizer(value) : patternGenerator(value);
		}
	}

	public class StaticSwitchRule : UpperRule, ISwitchRule
	{
		public StaticSwitchRule(PropertyInfo propertyInfo = null, Rule defaultRule = null, params Case<IPattern>[] cases)
		{
			PropertyInfo = propertyInfo;
			DefaultRule = defaultRule;
			Cases = cases.ToList();
		}

		public PropertyInfo PropertyInfo { get; set; }

		public List<Case<IPattern>> Cases { get; }

		public Rule DefaultRule { get; set; }

		IEnumerable<ICase<IPattern>> ISwitchRule.Cases => Cases;

		IRule ISwitchRule.DefaultRule => DefaultRule;

		public override IEnumerable<(string, Rule)> Children => Cases.Select(c => (c.ToString(), c.Output)).With(($"Other {PropertyInfo}", DefaultRule));

		public static StaticSwitchRule Parse(XElement element)
		{
			PropertyInfo propertyInfo = null;
			Rule defaultRule = null;
			var cases = new List<Case<IPattern>>();
			var list = new List<IPattern>();
			string name = null;
			IPattern<Context> where = null;

			foreach (var child in element.Elements())
			{
				switch (child.Name.ToString())
				{
					case "Name":
						name = child.Value;
						break;

					case "On":
						propertyInfo = PropertyInfo.Parse<Context>(element.Element("On"));
						break;

					case "Case":
						list.Add(RoR2PatternParser.Instance.Parse(propertyInfo.Type, child.OnlyChild()));
						break;

					case "Where":
						where = RoR2PatternParser.Instance.Parse<Context>(child.OnlyChild());
						break;

					case "Return":
						cases.Add(new Case<IPattern>(FromXml(child.OnlyChild()), where, list.ToArray()).Named(name));
						where = null;
						name = null;
						list.Clear();
						break;

					case "Default":
						defaultRule = FromXml(child.OnlyChild());
						break;

					default:
						throw new XmlException();
				}
			}

			return new StaticSwitchRule(propertyInfo, defaultRule, cases.ToArray());
		}

		public override Rule GetRule(Context c)
		{
			var seenValue = c.GetPropertyValue(PropertyInfo.Name);

			foreach (var @case in Cases)
			{
				if (@case.WherePattern?.IsMatch(c) != false)
				{
					if (@case.Arr.Any(pattern => pattern.IsMatch(seenValue)))
					{
						return @case.Output;
					}
				}
			}

			return DefaultRule;
		}

		public override XElement ToXml()
		{
			var element = base.ToXml();
			var onElement = new XElement("On");
			PropertyInfo.AddAttributesTo(onElement);
			element.Add(onElement);
			foreach (var @case in Cases)
			{
				if (!(@case.Name is null))
				{
					element.Add(new XElement("Name", @case.Name));
				}

				foreach (var pattern in @case.Arr)
				{
					element.Add(new XElement("Case", pattern.ToXml()));
				}

				if (!(@case.WherePattern is null))
				{
					element.Add(new XElement("Where", @case.WherePattern.Simplify().ToXml()));
				}

				element.Add(new XElement("Return", @case.Output.ToXml()));
			}

			if (!(DefaultRule is null))
			{
				element.Add(new XElement("Default", DefaultRule.ToXml()));
			}

			return element;
		}

		public override IReadOnlyRule ToReadOnly() => new ReadOnlySwitchRule(this);
	}
}