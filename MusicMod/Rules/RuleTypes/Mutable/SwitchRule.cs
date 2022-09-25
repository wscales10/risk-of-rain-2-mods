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

    public static class StaticSwitchRule
    { }

    public class SwitchRule<T, TContext, TOut> : UpperRule<TContext, TOut>
    {
        private readonly RuleParser<TContext, TOut> ruleParser;

        private readonly PatternGenerator<T> patternGenerator;

        public SwitchRule(string propertyName, params ICaseGetter<T, TContext, TOut>[] cases)
        {
            PropertyName = propertyName;
            Cases = cases.SelectMany(c => c.GetCases()).ToReadOnlyCollection();
        }

        public SwitchRule(string propertyName, PatternGenerator<T> patternGenerator, params ICaseGetter<T, TContext, TOut>[] cases) : this(propertyName, cases)
        {
            this.patternGenerator = patternGenerator;
        }

        public SwitchRule(string propertyName, Rule<TContext, TOut> defaultRule, params ICaseGetter<T, TContext, TOut>[] cases) : this(propertyName, cases)
        {
            DefaultRule = defaultRule;
        }

        public SwitchRule(string propertyName, Rule<TContext, TOut> defaultRule, PatternGenerator<T> patternGenerator, params ICaseGetter<T, TContext, TOut>[] cases) : this(propertyName, cases)
        {
            DefaultRule = defaultRule;
            this.patternGenerator = patternGenerator;
        }

        public string PropertyName { get; }

        public ReadOnlyCollection<RuleCase<T, TContext, TOut>> Cases { get; }

        public Rule<TContext, TOut> DefaultRule { get; }

        public ArrayRule<TContext, TOut> ToArrayRule()
        {
            var rules = Cases.Select(c =>
            {
                var pattern = new OrPattern<T>(c.Arr.Select(GeneratePattern).ToArray());
                return new IfRule<TContext, TOut>(PropertyPattern<TContext>.Create(PropertyName, pattern) & (c.WherePattern ?? ConstantPattern<TContext>.True), c.Output);
            }).Cast<Rule<TContext, TOut>>().ToList();

            if (!(DefaultRule is null))
                rules.Add(DefaultRule);

            return (ArrayRule<TContext, TOut>)new ArrayRule<TContext, TOut>(rules).Named(Name);
        }

        public StaticSwitchRule<TContext, TOut> ToStatic()
        {
            return (StaticSwitchRule<TContext, TOut>)new StaticSwitchRule<TContext, TOut>(
                new PropertyInfo(PropertyName, typeof(T)),
                DefaultRule,
                Cases.Select(c => (RuleCase<TContext, TOut>)new RuleCase<TContext, TOut>(c.Output, c.WherePattern, c.Arr.Select(GeneratePattern).ToArray()).Named(c.Name)).ToArray()).Named(Name);
        }

        public override IEnumerable<Rule<TContext, TOut>> GetRules(TContext c)
        {
            T seenValue = c.GetPropertyValue<T>(PropertyName);

            foreach (var @case in Cases)
            {
                if (@case.WherePattern?.IsMatch(c) != false)
                {
                    if (patternGenerator is null ? @case.Arr.Contains(seenValue) : @case.Arr.Any(allowedValue => patternGenerator(allowedValue).IsMatch(seenValue)))
                    {
                        yield return @case.Output;
                    }
                }
            }

            yield return DefaultRule;

            this.Log($"{nameof(seenValue)}: {seenValue}");
        }

        public override XElement ToXml()
        {
            // return ((ArrayRule)this).ToXml();
            return ToStatic().ToXml();
        }

        public override IReadOnlyRule<TContext, TOut> ToReadOnly() => ToStatic().ToReadOnly();

        private IPattern<T> GeneratePattern(T value)
        {
            return patternGenerator is null ? ruleParser.PatternParser.GetEqualizer(value) : patternGenerator(value);
        }
    }

    public class StaticSwitchRule<TContext, TOut> : UpperRule<TContext, TOut>, ISwitchRule<TContext, TOut>
    {
        public StaticSwitchRule(PropertyInfo propertyInfo = null, Rule<TContext, TOut> defaultRule = null, params RuleCase<TContext, TOut>[] cases)
        {
            PropertyInfo = propertyInfo;
            DefaultRule = defaultRule;
            Cases = cases.ToList();
        }

        public PropertyInfo PropertyInfo { get; set; }

        public List<RuleCase<TContext, TOut>> Cases { get; }

        public Rule<TContext, TOut> DefaultRule { get; set; }

        IEnumerable<ICase<IPattern, TContext, TOut>> ISwitchRule<TContext, TOut>.Cases => Cases.Cast<RuleCase<IPattern, TContext, TOut>>();

        IRule<TContext, TOut> ISwitchRule<TContext, TOut>.DefaultRule => DefaultRule;

        public override IEnumerable<(string, Rule<TContext, TOut>)> Children => Cases.Select(c => (c.ToString(), c.Output)).With(($"Other {PropertyInfo}", DefaultRule));

        public static StaticSwitchRule<TContext, TOut> Parse(XElement element, RuleParser<TContext, TOut> ruleParser)
        {
            PropertyInfo propertyInfo = null;
            Rule<TContext, TOut> defaultRule = null;
            var cases = new List<RuleCase<TContext, TOut>>();
            var list = new List<IPattern>();
            string name = null;
            IPattern<TContext> where = null;

            foreach (var child in element.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "Name":
                        name = child.Value;
                        break;

                    case "On":
                        propertyInfo = PropertyInfo.Parse<TContext>(element.Element("On"));
                        break;

                    case "Case":
                        list.Add(ruleParser.PatternParser.Parse(propertyInfo?.Type, child.OnlyChild()));
                        break;

                    case "Where":
                        where = ruleParser.PatternParser.Parse<TContext>(child.OnlyChild());
                        break;

                    case "Return":
                        cases.Add((RuleCase<TContext, TOut>)new RuleCase<TContext, TOut>(ruleParser.Parse(child.OnlyChild()), where, list.ToArray()).Named(name));
                        where = null;
                        name = null;
                        list.Clear();
                        break;

                    case "Default":
                        defaultRule = ruleParser.Parse(child.OnlyChild());
                        break;

                    default:
                        throw new XmlException();
                }
            }

            return new StaticSwitchRule<TContext, TOut>(propertyInfo, defaultRule, cases.ToArray());
        }

        public override IEnumerable<Rule<TContext, TOut>> GetRules(TContext c)
        {
            var seenValue = c.GetPropertyValue(PropertyInfo.Name);

            foreach (var @case in Cases)
            {
                if (@case.WherePattern?.IsMatch(c) != false)
                {
                    if (@case.Arr.Any(pattern => pattern.IsMatch(seenValue)))
                    {
                        yield return @case.Output;
                    }
                }
            }

            yield return DefaultRule;

            this.Log($"{nameof(seenValue)}: {seenValue}");
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

        public override IReadOnlyRule<TContext, TOut> ToReadOnly() => new ReadOnlySwitchRule<TContext, TOut>(this);
    }
}