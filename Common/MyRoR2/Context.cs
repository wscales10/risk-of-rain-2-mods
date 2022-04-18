using RoR2;

namespace MyRoR2
{
	public struct Context
	{
		public MyScene SceneName { get; set; }

		public SceneType SceneType { get; set; }

		public int? StageNumber { get; set; }

		public int? WaveNumber { get; set; }

		public int? LoopIndex { get; set; }

		public string BossBodyName { get; set; }

		public bool IsBossEncounter { get; set; }

		public TeleporterInteraction.ActivationState? TeleporterState { get; set; }

		public int ScenePart { get; set; }

		public RunType RunType { get; set; }
	}
}
