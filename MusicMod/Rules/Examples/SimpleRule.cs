using MyRoR2;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;

namespace Rules
{
	using static RuleCase<RoR2Context, string>;

	public static class SimpleRule1
	{
		public static IReadOnlyRule<RoR2Context, string> Instance { get; } = Switcher.RoR2ToString.Create
		(
			nameof(RoR2Context.TeleporterState),
			new Bucket<RoR2Context, string>("Stop"),
			C<TeleporterState?>("Teleporter Charging", TeleporterState.Charging),
			C<TeleporterState?>("Teleporter Idle", TeleporterState.Idle)
		).ToReadOnly(RuleParser.RoR2ToString);
	}
}