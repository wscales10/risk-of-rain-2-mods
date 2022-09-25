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
    public class RuleParser<TContext, TOut>
    {
        private readonly Func<string, TOut> outputParser;

        public RuleParser(PatternParser patternParser, Func<string, TOut> outputParser)
        {
            PatternParser = patternParser ?? throw new ArgumentNullException(nameof(patternParser));
            this.outputParser = outputParser;
        }

        public PatternParser PatternParser { get; }

        public Rule<TContext, TOut> Parse(XElement element)
        {
            if (element is null)
            {
                return null;
            }

            var name = element.Attribute("name")?.Value;
            return GetUnnamed().Named(name);

            Rule<TContext, TOut> GetUnnamed()
            {
                switch (element.Attribute("type").Value)
                {
                    case nameof(ArrayRule):
                        return new ArrayRule<TContext, TOut>(element.Elements(nameof(Rule)).Select(Parse).ToArray());

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

                        return new IfRule<TContext, TOut>(pattern, Parse(element.Element("Then").OnlyChild()), Parse(element.Element("Else")?.OnlyChild()));

                    case nameof(Bucket):
                        return new Bucket<TContext, TOut>(outputParser(element.Value));

                    case nameof(StaticSwitchRule):
                        return StaticSwitchRule<TContext, TOut>.Parse(element, this);

                    default:
                        throw new XmlException();
                }
            }
        }

        public Rule<TContext, TOut> DeepClone(Rule<TContext, TOut> rule) => Parse(rule.ToXml());
    }
}