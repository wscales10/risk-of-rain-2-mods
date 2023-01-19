using MyRoR2;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;

namespace Rules
{
	using static RuleCase<Context, string>;

	public static class SimpleRule1
	{
		public static IReadOnlyRule<Context, string> Instance { get; } = Switcher.RoR2ToString.Create
		(
			nameof(Context.TeleporterState),
			new Bucket<Context, string>("Stop"),
			C<TeleporterState?>("Teleporter Charging", TeleporterState.Charging),
			C<TeleporterState?>("Teleporter Idle", TeleporterState.Idle)
		).ToReadOnly(RuleParser.RoR2ToString);
	}
}