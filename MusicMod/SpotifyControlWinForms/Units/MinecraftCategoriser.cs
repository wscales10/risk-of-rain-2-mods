using Minecraft;
using RuleExamples;
using RuleExamples.Minecraft;
using Rules.RuleTypes.Interfaces;

namespace SpotifyControlWinForms.Units
{
	internal class MinecraftCategoriser : RuleUnit<MinecraftContext, string>
	{
		private MinecraftCategoriser(string name) : base(name, RuleParsers.MinecraftToString)
		{
		}

		public static MinecraftCategoriser Instance { get; } = new(nameof(MinecraftCategoriser));

		public override IRule<MinecraftContext, string>? DefaultRule => MinecraftRule.Instance;
	}
}