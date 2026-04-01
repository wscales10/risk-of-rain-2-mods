using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2
{
    public class Halcyonite2 : Stage1HalcyoniteBossWaveDefinition
    {
        private readonly AssetPromise<CharacterSpawnCard> solusProspectorSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC3/WorkerUnit/cscWorkerUnit.asset");

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);

            wavePrefab.EnsureComponent<Halcyonite2BossFightBehavior>();
            wavePrefab.EnsureComponent<KeepCombatDirectorEnabledBehavior>();

            dir.SetupCombatDirectorPrefabForAddsSpawning(new AddsSpawningArgs
            {
                MonsterCards = Utils.MakeDirectorCardCategorySelection(
                    ("Melee", new[] { this.solusProspectorSpawnCard.Value })
                ),
                MaxSquadCount = 3,
                SpawnFrequency = 20,
                SpawnFrequencyVariation = 1,
                ExpectedDifficultyCoefficient = 3.06942795f
            });
        }
    }
}
