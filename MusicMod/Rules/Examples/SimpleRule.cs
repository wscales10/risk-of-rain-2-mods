using MyRoR2;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using static RoR2.TeleporterInteraction;

namespace Rules
{
	using static RuleCase<Context, string>;

	public static class SimpleRule1
	{
		public static IReadOnlyRule<Context, string> Instance { get; } = Switcher.RoR2ToString.Create
		(
			nameof(Context.TeleporterState),
			new Bucket<Context, string>("Stop"),
			C<ActivationState?>("Teleporter Charging", ActivationState.Charging),
			C<ActivationState?>("Teleporter Idle", ActivationState.Idle)
		).ToReadOnly(RuleParser.RoR2ToString);
	}
}