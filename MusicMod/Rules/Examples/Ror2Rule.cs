using MyRoR2;
using Patterns.Patterns;
using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;

namespace Rules.Examples
{
	using static RuleCase<Context, string>;
	using Query = Query<Context>;

	public static class Ror2Rule
	{
		private static readonly Rule<Context, string> IdleRule = Switcher.RoR2ToString.Create(nameof(Context.SceneName), ScenePattern.Equals,
					C<MyScene>("Abandoned Aqueduct (Simulacrum)", Query.Create<RunType>(nameof(Context.RunType), EnumRangePattern.Equals(RunType.Simulacrum)), Scenes.AbandonedAqueduct),
					M("Idle", Scenes.DistantRoost),
					M("Idle", Scenes.AbandonedAqueduct, Scenes.AbyssalDepths),
					M("Idle", Scenes.TitanicPlains, Scenes.ScorchedAcres),
					M("Idle", Scenes.RallypointDelta, Scenes.GildedCoast),
					M("Idle", Scenes.VoidFields),
					M("Idle", Scenes.WetlandAspect),
					M("Idle", Scenes.BazaarBetweenTime),
					M("Idle", Scenes.SkyMeadow),
					M("Idle", Scenes.SirensCall, Scenes.BulwarksAmbry),
					M("Idle", Scenes.AMomentFractured, Scenes.AMomentWhole),
					M("Idle", Scenes.VoidLocus),
					M("Idle", Scenes.SiphonedForest),
					M("Idle", Scenes.AphelianSanctuary),
					M("Idle", Scenes.SunderedGrove),
					M("Idle", Scenes.SulfurPools)
			).Named("Other");

		private static readonly Rule<Context, string> SpecialRule = Switcher.RoR2ToString.Create(
			nameof(Context.SceneName),
			ScenePattern.Equals,
			C<MyScene>(IfRule.Create(
				Query.Create<bool>(nameof(Context.IsBossEncounter), BoolPattern.True),
				Switcher.RoR2ToString.Create(
					nameof(Context.ScenePart),
					C("Mithrix Phase 1", 0).Named("Phase 1"),
					C("Mithrix Phase 2", 1).Named("Phase 2"),
					C("Mithrix Phase 3", 2).Named("Phase 3"),
					C("Mithrix Phase 4", 3).Named("Phase 4"),
					C("Mithrix Death Animation", 4).Named("Death Animation")),
				"Moon escape sequence").Named("Commencement"), Scenes.Commencement),
			C<MyScene>(IfRule.Create<Context, string>(
				Query.Create<bool>(nameof(Context.IsBossEncounter), BoolPattern.True),
				"Voidling Fight",
				"The Planetarium (Idle)").Named("The Planetarium"), Scenes.ThePlanetarium)).Named("Final Stages");

		private static readonly Rule<Context, string> BossRule = Switcher.RoR2ToString.Create(
			nameof(Context.SceneName),
			ScenePattern.Equals,
			M("Teleporter", Scenes.TitanicPlains, Scenes.DistantRoost, Scenes.AbandonedAqueduct),
			M("Teleporter", Scenes.WetlandAspect, Scenes.ScorchedAcres, Scenes.SirensCall),
			M("Teleporter", Scenes.RallypointDelta, Scenes.AbyssalDepths, Scenes.SunderedGrove),
			M("Teleporter", Scenes.SkyMeadow),
			M("Teleporter", Scenes.SiphonedForest, Scenes.AphelianSanctuary),
			M("Teleporter", Scenes.SulfurPools));

		private static readonly Rule<Context, string> EnvironmentRule = Switcher.RoR2ToString.Create(
			nameof(Context.TeleporterState),
			C<TeleporterState?>("Teleporter Charged", TeleporterState.Charged),
			C<TeleporterState?>(BossRule, TeleporterState.IdleToCharging, TeleporterState.Charging).Named("Teleporter"),
			C<TeleporterState?>(ArrayRule.Create(SpecialRule, IdleRule), TeleporterState.Idle, null).Named("Other"));

		private static readonly Rule<Context, string> OtherRule = Switcher.RoR2ToString.Create(
			nameof(Context.SceneName),
			ScenePattern.Equals,
			C<MyScene>("Splash and Intro", Scenes.SplashScreen, Scenes.IntroCutscene),
			C<MyScene>("Main Menu", Scenes.MainMenu),
			C<MyScene>(IfRule.Create<Context, string>(Query.Create(nameof(Context.ScenePart), IntPattern.x == 0), "Outro", "Credits"), Scenes.Outro),
			C<MyScene>("Logbook", Scenes.Logbook),
			C<MyScene>("Normal Character Select", Query.Create<RunType>(nameof(Context.RunType), EnumRangePattern.Equals(RunType.Normal)), Scenes.CharacterSelect),
			C<MyScene>("Eclipse Menus", Query.Create<RunType>(nameof(Context.RunType), EnumRangePattern.Equals(RunType.Eclipse)), Scenes.EclipseMenu, Scenes.CharacterSelect),
			C<MyScene>("Prismatic Trials Menus", Query.Create<RunType>(nameof(Context.RunType), EnumRangePattern.Equals(RunType.PrismaticTrial)), Scenes.PrismaticTrialsMenu, Scenes.CharacterSelect),
			C<MyScene>("Simulacrum Menus", Query.Create<RunType>(nameof(Context.RunType), EnumRangePattern.Equals(RunType.Simulacrum)), Scenes.SimulacrumMenu, Scenes.CharacterSelect));

		public static IReadOnlyRule<Context, string> Instance { get; } = Switcher.RoR2ToString.Create(
			nameof(Context.SceneType),
			OtherRule,
			C(EnvironmentRule, SceneType.Stage, SceneType.Intermission).Named("Environments")).ToReadOnly(RuleParser.RoR2ToString);

		private static MultiCase<MyScene, Context, string> M(string descriptor, params DefinedScene[] sceneNames)
		{
			return new MultiCase<MyScene, Context, string>((sceneName) => $"{sceneName} ({descriptor})", sceneNames);
		}
	}
}