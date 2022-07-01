using MyRoR2;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using static RoR2.TeleporterInteraction;

namespace Rules
{
    using static Rules.RuleTypes.Mutable.RuleCase<Context>;

    public static partial class Examples
    {
        public static IReadOnlyRule<Context> SimpleRule { get; } = new SwitchRule<ActivationState?, Context>
        (
            nameof(Context.TeleporterState),
            new StopCommand(),
            C<ActivationState?>(Bucket<Context>.Play("0VlxbRzdOyA56BviBK9vkc"), ActivationState.Charging),
            C<ActivationState?>(Bucket<Context>.Play("6Tfz4vOltixQpJFNaFvQ1H"), ActivationState.Idle)
        ).ToReadOnly();
    }
}