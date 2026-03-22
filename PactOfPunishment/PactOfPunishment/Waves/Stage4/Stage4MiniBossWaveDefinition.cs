using HG;
using PactOfPunishment.Waves.Common;
using RoR2;

namespace PactOfPunishment.Waves.Stage4
{
    public abstract class Stage4MiniBossWaveDefinition : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        protected abstract uint BaseMaxSquadCount { get; }

        protected abstract float AddSpawnInterval { get; }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);

            wavePrefab.spawnList = new InfiniteTowerExplicitSpawnWaveController.SpawnInfo[]
            {
                new InfiniteTowerExplicitSpawnWaveController.SpawnInfo
                {
                    count = 1,
                    spawnCard = this.GetBossSpawnCard(Stage4MiniBossSpawnCards.Instance).Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Close,
                },
            };

            wavePrefab.EnsureComponent<KeepCombatDirectorEnabledBehavior>();
            dir.SetupCombatDirectorPrefabForAddsSpawning(this.GetAddsMonsterCards(Stage4AddsSpawnCards.Instance), this.BaseMaxSquadCount, this.AddSpawnInterval, this.AddSpawnInterval / 5, 15.6152565f);
            dir.EnsureComponent<Stage4MiniBossFightBehavior>();
        }

        protected abstract DirectorCardCategorySelection GetAddsMonsterCards(Stage4AddsSpawnCards cards);

        protected abstract AssetPromise<CharacterSpawnCard> GetBossSpawnCard(Stage4MiniBossSpawnCards cards);
    }
}