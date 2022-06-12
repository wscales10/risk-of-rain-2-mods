using RoR2;
using System.Collections.Generic;

namespace MyRoR2
{
    public struct Context
    {
        public MyScene SceneName { get; set; }

        public SceneType SceneType { get; set; }

        public int? StageNumber { get; set; }

        public int? WaveNumber { get; set; }

        public int? LoopIndex { get; set; }

        public Entity BossBodyName { get; set; }

        public IList<Entity> Bosses { get; set; }

        public bool IsBossEncounter { get; set; }

        public TeleporterInteraction.ActivationState? TeleporterState { get; set; }

        public int ScenePart { get; set; }

        public RunType RunType { get; set; }

        public Entity Survivor { get; set; }

        public RunOutcome? Outcome { get; set; }
    }
}