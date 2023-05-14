using MyRoR2;
using SpotifyControlWinForms.Connections;
using System.Text.RegularExpressions;
using Utils;

namespace SpotifyControlWinForms.Units
{
	internal class OverwatchMithrixHandler : Unit<OverwatchConnection.LogEntry, RoR2Context?>
	{
		private static readonly Regex regex = new(@"^BroadcastState: (?'state'.*)$");

		private RoR2Context cachedOutput;

		private OverwatchMithrixHandler(string name) : base(name)
		{
		}

		public static OverwatchMithrixHandler Instance { get; } = new("Overwatch Mithrix Handler");

		protected override RoR2Context? Transform(OverwatchConnection.LogEntry input)
		{
			var output = transform(input);

			if (output is not null)
			{
				cachedOutput = output.Value;
			}

			return output;
		}

		private RoR2Context? transform(OverwatchConnection.LogEntry input)
		{
			if (input.Text == string.Empty)
			{
				return default(RoR2Context);
			}

			var match = regex.Match(input.Text);

			if (match.Success)
			{
				switch (match.Groups["state"].Value)
				{
					case "Waiting":
						return new RoR2Context()
						{
							SceneName = Scenes.Commencement,
							SceneType = SceneType.Stage,
							StageNumber = 6,
							LoopIndex = 1,
							Bosses = Enumerable.Empty<Entity>().ToReadOnlyCollection(),
							ScenePart = 0,
							RunType = RunType.Normal,
							Outcome = RunOutcome.Undecided
						};

					case "OnTheWay":
						return new RoR2Context()
						{
							SceneName = Scenes.Commencement,
							SceneType = SceneType.Stage,
							StageNumber = 6,
							LoopIndex = 1,
							Bosses = Enumerable.Empty<Entity>().ToReadOnlyCollection(),
							ScenePart = 1,
							RunType = RunType.Normal,
							Outcome = RunOutcome.Undecided
						};

					case "Phase 1":
						return new RoR2Context()
						{
							SceneName = Scenes.Commencement,
							SceneType = SceneType.Stage,
							StageNumber = 6,
							LoopIndex = 1,
							IsBossEncounter = true,
							Bosses = new Entity[] { Entities.Mithrix }.ToReadOnlyCollection(),
							ScenePart = 1,
							RunType = RunType.Normal,
							Outcome = RunOutcome.Undecided
						};

					case "Phase 2":
						return new RoR2Context()
						{
							SceneName = Scenes.Commencement,
							SceneType = SceneType.Stage,
							StageNumber = 6,
							LoopIndex = 1,
							IsBossEncounter = true,
							Bosses = Enumerable.Empty<Entity>().ToReadOnlyCollection(),
							ScenePart = 2,
							RunType = RunType.Normal,
							Outcome = RunOutcome.Undecided
						};

					case "Phase 3":
						return new RoR2Context()
						{
							SceneName = Scenes.Commencement,
							SceneType = SceneType.Stage,
							StageNumber = 6,
							LoopIndex = 1,
							IsBossEncounter = true,
							Bosses = new Entity[] { Entities.Mithrix }.ToReadOnlyCollection(),
							ScenePart = 3,
							RunType = RunType.Normal,
							Outcome = RunOutcome.Undecided
						};

					case "Phase 4":
						return new RoR2Context()
						{
							SceneName = Scenes.Commencement,
							SceneType = SceneType.Stage,
							StageNumber = 6,
							LoopIndex = 1,
							IsBossEncounter = true,
							Bosses = new Entity[] { Entities.Mithrix }.ToReadOnlyCollection(),
							ScenePart = 4,
							RunType = RunType.Normal,
							Outcome = RunOutcome.Undecided
						};

					case "BossVictory":
						return new RoR2Context()
						{
							SceneName = Scenes.Commencement,
							SceneType = SceneType.Stage,
							StageNumber = cachedOutput.StageNumber,
							LoopIndex = cachedOutput.LoopIndex,
							Bosses = cachedOutput.Bosses,
							IsBossEncounter = true,
							ScenePart = cachedOutput.ScenePart,
							RunType = cachedOutput.RunType,
							Survivor = cachedOutput.Survivor,
							Outcome = RunOutcome.Defeat
						};

					case "PlayerVictory":
						return new RoR2Context()
						{
							SceneName = Scenes.Commencement,
							SceneType = SceneType.Stage,
							StageNumber = 6,
							LoopIndex = 1,
							IsBossEncounter = true,
							Bosses = Enumerable.Empty<Entity>().ToReadOnlyCollection(),
							ScenePart = 5,
							RunType = RunType.Normal,
							Outcome = RunOutcome.Undecided
						};
				}
			}

			return null;
		}
	}
}