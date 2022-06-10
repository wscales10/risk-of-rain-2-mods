using MyRoR2;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify.Commands.Mutable;
using static RoR2.TeleporterInteraction;
using static Rules.RuleTypes.Mutable.Case;

namespace Rules
{
    public static partial class Examples
    {
        public static IReadOnlyRule SimpleRule { get; } = new SwitchRule<ActivationState?>
        (
            nameof(Context.TeleporterState),
            new StopCommand(),
            C<ActivationState?>(Bucket.Play("0VlxbRzdOyA56BviBK9vkc"), ActivationState.Charging),
            C<ActivationState?>(Bucket.Play("6Tfz4vOltixQpJFNaFvQ1H"), ActivationState.Idle)
        ).ToReadOnly();
    }
}