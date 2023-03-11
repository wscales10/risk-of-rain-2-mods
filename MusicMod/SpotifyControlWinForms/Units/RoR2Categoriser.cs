using MyRoR2;
using RuleExamples;
using RuleExamples.RiskOfRain2;
using Rules.RuleTypes.Interfaces;

namespace SpotifyControlWinForms.Units
{
	internal class RoR2Categoriser : RuleUnit<RoR2Context, string>
	{
		private RoR2Categoriser(string name) : base(name, RuleParsers.RoR2ToString)
		{
		}

		public static RoR2Categoriser Instance { get; } = new(nameof(RoR2Categoriser));

		public override IRule<RoR2Context, string> DefaultRule => Ror2Rule.Instance;
	}
}