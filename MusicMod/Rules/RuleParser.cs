using Patterns;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Utils;
using System;

namespace Rules
{
    public class RuleParser<TContext>
    {
        public RuleParser(PatternParser patternParser)
        {
            PatternParser = patternParser ?? throw new ArgumentNullException(nameof(patternParser));
        }

        public PatternParser PatternParser { get; }

        public Rule<TContext> Parse(XElement element)
        {
            if (element is null)
            {
                return null;
            }

            var name = element.Attribute("name")?.Value;
            return GetUnnamed().Named(name);

            Rule<TContext> GetUnnamed()
            {
                switch (element.Attribute("type").Value)
                {
                    case nameof(ArrayRule):
                        return new ArrayRule<TContext>(element.Elements(nameof(Rule)).Select(Parse).ToArray());

                    case nameof(IfRule):
                        var ifElement = element.Elements().First();
                        IPattern<TContext> pattern;

                        try
                        {
                            pattern = PatternParser.Parse<TContext>(ifElement);
                        }
                        catch (XmlException)
                        {
                            pattern = null;
                        }

                        return new IfRule<TContext>(pattern, Parse(element.Element("Then").OnlyChild()), Parse(element.Element("Else")?.OnlyChild()));

                    case nameof(Bucket):
                        return new Bucket<TContext>(element.Elements().Select(Command.FromXml).ToList());

                    case nameof(StaticSwitchRule):
                        return StaticSwitchRule<TContext>.Parse(element, this);

                    default:
                        throw new XmlException();
                }
            }
        }

        public Rule<TContext> DeepClone(Rule<TContext> rule) => Parse(rule.ToXml());
    }
}