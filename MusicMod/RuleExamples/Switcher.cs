using Minecraft;
using MyRoR2;
using Rules;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;

namespace RuleExamples
{
	public static class Switcher
	{
		public static Switcher<RoR2Context, string> RoR2ToString { get; } = Create(RuleParsers.RoR2ToString);

		public static Switcher<string, ICommandList> StringToSpotify { get; } = Create(RuleParsers.StringToSpotify);

		public static Switcher<MinecraftContext, string> MinecraftToString { get; } = Create(RuleParsers.MinecraftToString);

		public static Switcher<TContext, TOut> Create<TContext, TOut>(RuleParser<TContext, TOut> ruleParser)
		{
			return new Switcher<TContext, TOut>(ruleParser);
		}
	}
}