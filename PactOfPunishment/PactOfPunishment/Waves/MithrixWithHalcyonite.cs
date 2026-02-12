using PactOfPunishment.Conditions;
using RoR2;
using System;
using UnityEngine.AddressableAssets;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves
{
    public class MithrixWithHalcyonite : MainBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        protected override string BaseWavePrefabKey => "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveBossBrother.prefab";

        protected override UpgradeWaveStrategy GetUpgradeMainBossStrategy()
        {
            throw new NotImplementedException();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/cscBrotherIT.asset").WaitForCompletion()
                },
                new SpawnInfo
                {
                    count = 1,
                    eliteDef = DLC3Content.Elites.Collective,
                    spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC2/Halcyonite/cscHalcyonite.asset").WaitForCompletion()
                }
            };
        }
    }
}