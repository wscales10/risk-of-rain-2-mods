using Patterns;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Utils;
using System;
using MyRoR2;

namespace Rules
{
	public static class RuleParser
	{
		// TODO: move elsewhere
		public static RuleParser<Context, string> RoR2ToString { get; } = new RuleParser<Context, string>(RoR2PatternParser.Instance, s => s);

		public static RuleParser<string, ICommandList> StringToSpotify { get; } = new RuleParser<string, ICommandList>(RoR2PatternParser.Instance, s =>
		{
			return CommandList.Parse(XElement.Parse(s));
		});
	}

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
						var ifElement = element.Elements().FirstOrDefault();
						IPattern<TContext> pattern;

						if (ifElement is null)
						{
							pattern = null;
						}
						else
						{
							try
							{
								pattern = PatternParser.Parse<TContext>(ifElement);
							}
							catch (XmlException)
							{
								pattern = null;
							}
						}

						return new IfRule<TContext, TOut>(pattern, Parse(element.Element("Then")?.OnlyChild()), Parse(element.Element("Else")?.OnlyChild()));

					case nameof(Bucket):
						return new Bucket<TContext, TOut>(outputParser(element.FirstNode.ToString()));

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