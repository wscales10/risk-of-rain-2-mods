using MyRoR2;
using Patterns.Patterns.SmallPatterns.ValuePatterns;
using RoR2;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify.Commands;
using static Patterns.Case;
using static RoR2.TeleporterInteraction;

namespace Rules
{
    using static RuleCase<Context, ICommandList>;
    using static Bucket<Context, ICommandList>;
    using Rule = Rule<Context, ICommandList>;
    using IfRule = IfRule<Context, ICommandList>;
    using ArrayRule = ArrayRule<Context, ICommandList>;

    using Bucket = Bucket<Context, ICommandList>;

    public static partial class Examples
    {
        private static readonly Rule IdleRule = Switcher.Instance.Create<MyScene>(nameof(Context.SceneName), ScenePattern.Equals,
                    C<MyScene>(Play("0YPqWkd7ad07IJfaGDgBLE"), Query.Create<RunType>(nameof(Context.RunType), EnumRangePattern.Equals(RunType.Simulacrum)), Scenes.AbandonedAqueduct),
                    M("7qOPvD6VE4EsC6LU3VBCOR", Scenes.DistantRoost),
                    M("0ViueDwpuVR94Ny75ItU1P", Scenes.AbandonedAqueduct, Scenes.AbyssalDepths),
                    M("3NlFc4z24XefS6Orsz7zVG", Scenes.TitanicPlains, Scenes.ScorchedAcres),
                    M("3O4t4NB2N0RnPazn3Jf06I", Scenes.RallypointDelta, Scenes.GildedCoast),
                    M("2C3Nzw3gkEQaS6pXdeKG8Y", Scenes.VoidFields),
                    M("4oLjFQGMuljNXPVXDYlCjQ", Scenes.WetlandAspect),
                    M("6aBPlHW5g28spdHdvY19kb", Scenes.BazaarBetweenTime),
                    M("5KEoPtv1mCNpF9Ne75kozY", Scenes.SkyMeadow),
                    M("5tWWzN2tXSCOdinN6eOabW", Scenes.SirensCall, Scenes.BulwarksAmbry),
                    M("5cCjhYmgJwQwm5eEgttDxC", Scenes.AMomentFractured, Scenes.AMomentWhole),
                    M("2pl3Mzh2LeeUyzFacnHyZc", Scenes.VoidLocus),
                    M("75ENq958lhxLehA24dz72n", Scenes.SiphonedForest),
                    M("5MtbLsBAmW4VmXQvVfkvtc", Scenes.AphelianSanctuary),
                    M("6MiEIwSj9Z4UbxBdjxtBFB", Scenes.SunderedGrove),
                    M("0YPqWkd7ad07IJfaGDgBLE", Scenes.SulfurPools)
            ).Named("Other");

        private static readonly Rule SpecialRule = Switcher.Instance.Create<MyScene>(
            nameof(Context.SceneName),
            ScenePattern.Equals,
            C<MyScene>(new IfRule(
                Query.Create<bool>(nameof(Context.IsBossEncounter), BoolPattern.True),
                Switcher.Instance.Create<int>(
                    nameof(Context.ScenePart),
                    C(Play("3KE5ossIfDAnBqNJFF8LfF", 6280), 0).Named("Phase 1"),
                    C(Play("3KE5ossIfDAnBqNJFF8LfF", 85675), 1).Named("Phase 2"),
                    C(Play("3KE5ossIfDAnBqNJFF8LfF", 148045), 2).Named("Phase 3"),
                    C(Play("3KE5ossIfDAnBqNJFF8LfF", 259950), 3).Named("Phase 4"),
                    C(new CommandList(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Off }), 4).Named("Death Animation")),
                Play("7G349JbUH3PRdj5e7780Iz").Named("Escape sequence")).Named("Commencement"), Scenes.Commencement),
            C<MyScene>(new IfRule(
                Query.Create<bool>(nameof(Context.IsBossEncounter), BoolPattern.True),
                Play("5fdiSSxvIsrCJ7sVkuhxnD"),
                Play("2pl3Mzh2LeeUyzFacnHyZc")).Named("The Planetarium"), Scenes.ThePlanetarium)).Named("Final Stages");

        private static readonly Rule BossRule = Switcher.Instance.Create<MyScene>(
            nameof(Context.SceneName),
            ScenePattern.Equals,
            M("0j6CuhzD0XQlA5iTiRkx5P", Scenes.TitanicPlains, Scenes.DistantRoost, Scenes.AbandonedAqueduct),
            M("4pcp0D8SyvuIans1NOtTht", Scenes.WetlandAspect, Scenes.ScorchedAcres, Scenes.SirensCall),
            M("1XMfSk9yWjZdUNo1WuEIWF", Scenes.RallypointDelta, Scenes.AbyssalDepths, Scenes.SunderedGrove),
            M("0F5ZMhyD9Msd7mRSFSHgY5", Scenes.SkyMeadow),
            M("0G44J59yRCMqJp5k8JVYcz", Scenes.SiphonedForest, Scenes.AphelianSanctuary),
            M("6uwL79qmtMJZA3Cxxd94c7", Scenes.SulfurPools));

        private static readonly Rule EnvironmentRule = Switcher.Instance.Create<ActivationState?>(
            nameof(Context.TeleporterState),
            C<ActivationState?>(SeekToCommand.AtSeconds(-23).Then(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Off }), ActivationState.Charged).Named("Teleporter Charged"),
            C<ActivationState?>(BossRule, ActivationState.IdleToCharging, ActivationState.Charging).Named("Teleporter"),
            C<ActivationState?>(new ArrayRule(SpecialRule, IdleRule), ActivationState.Idle, null).Named("Other"));

        private static readonly Rule OtherRule = Switcher.Instance.Create<MyScene>(
            nameof(Context.SceneName),
            ScenePattern.Equals,
            C<MyScene>(Play("6H6LnqmjEcW8gl6D7dIRKJ"), Scenes.SplashScreen, Scenes.IntroCutscene),
            C<MyScene>(Main(), Scenes.MainMenu),
            C<MyScene>(new IfRule(Query.Create(nameof(Context.ScenePart), IntPattern.x == 0), Play("3tEnANZ8EeFj3FDBEYXQxG"), Play("5cCjhYmgJwQwm5eEgttDxC", 42000)), Scenes.Outro),
            C<MyScene>(Dehydrated(), Scenes.Logbook),
            C<MyScene>(Dehydrated(), Query.Create<RunType>(nameof(Context.RunType), EnumRangePattern.Equals(RunType.Normal)), Scenes.CharacterSelect),
            C<MyScene>(Dehydrated(), Query.Create<RunType>(nameof(Context.RunType), EnumRangePattern.Equals(RunType.Eclipse)), Scenes.EclipseMenu, Scenes.CharacterSelect),
            C<MyScene>(Dehydrated(), Query.Create<RunType>(nameof(Context.RunType), EnumRangePattern.Equals(RunType.PrismaticTrial)), Scenes.PrismaticTrialsMenu, Scenes.CharacterSelect),
            C<MyScene>(Play("6umrQw3KWPFS6CQHN7J5BW"), Query.Create<RunType>(nameof(Context.RunType), EnumRangePattern.Equals(RunType.Simulacrum)), Scenes.SimulacrumMenu, Scenes.CharacterSelect));

        public static IReadOnlyRule<Context, ICommandList> MimicRule { get; } = Switcher.Instance.Create<SceneType>(
            nameof(Context.SceneType),
            OtherRule,
            C(EnvironmentRule, SceneType.Stage, SceneType.Intermission).Named("Environments")).ToReadOnly(RuleParser.RoR2ToSpotify);

        private static Bucket Dehydrated() => Transfer("3BBYYGOlrDKEVOQvXD4huf", Switch("ms", P<int, string>("ms - 8000", IntPattern.x > 68000)), StringPattern.Equals("0Q4NqUpiuOnQxYOGjnbuCh"));

        private static Bucket Main() => Transfer("0Q4NqUpiuOnQxYOGjnbuCh", Switch("ms", P<int, string>("ms + 8000", IntPattern.x > 60000)), StringPattern.Equals("3BBYYGOlrDKEVOQvXD4huf"));

        private static MultiCase<MyScene, Context, ICommandList> M(string trackId, params DefinedScene[] sceneNames)
        {
            return new MultiCase<MyScene, Context, ICommandList>(() => Play(trackId), sceneNames);
        }
    }
}