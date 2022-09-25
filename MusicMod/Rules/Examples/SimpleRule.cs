using MyRoR2;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using static RoR2.TeleporterInteraction;

namespace Rules
{
    using static Rules.RuleTypes.Mutable.RuleCase<Context, ICommandList>;

    public static partial class Examples
    {
        public static IReadOnlyRule<Context, ICommandList> SimpleRule { get; } = new SwitchRule<ActivationState?, Context, ICommandList>
        (
            nameof(Context.TeleporterState),
            new CommandList(new StopCommand()),
            C<ActivationState?>(Bucket<Context, ICommandList>.Play("0VlxbRzdOyA56BviBK9vkc"), ActivationState.Charging),
            C<ActivationState?>(Bucket<Context, ICommandList>.Play("6Tfz4vOltixQpJFNaFvQ1H"), ActivationState.Idle)
        ).ToReadOnly();
    }
}