using EntityStates;
using EntityStates.BrotherMonster;
using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using RoR2;
using UnityEngine;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves.Stage3
{
    public class MithrixWithHalcyonite : MainBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private readonly AssetPromise<CharacterSpawnCard> mithrixSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/cscBrotherIT.asset");

        private readonly AssetPromise<CharacterSpawnCard> halcyoniteSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Halcyonite/cscHalcyonite.asset");

        protected override string BaseWavePrefabKey => "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveBossBrother.prefab";

        protected override UpgradeWaveStrategy GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<UpgradeMithrixAndHalcyonite>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = this.mithrixSpawnCard.Value,
                },
                new SpawnInfo
                {
                    count = 1,
                    eliteDef = DLC3Content.Elites.Collective,
                    spawnCard = this.halcyoniteSpawnCard.Value,
                }
            };
            wavePrefab.EnsureComponent<MithrixWithHalcyoniteBehavior>();
        }

        public class UpgradeMithrixAndHalcyonite : UpgradeWaveStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MainBoss;

            public override void PostInitialise(InfiniteTowerWaveController wave)
            {
                wave.combatDirector.AddSpawnListener(OnBossSpawnedServer);
                wave.EnsureComponent<PhaseCounter>().phase = 3;
            }

            private static void OnBossSpawnedServer(GameObject spawnedEntity)
            {
                var body = Utils.GetCharacterBody(spawnedEntity);

                if (body && body.name.Contains("Brother"))
                {
                    body.EnsureComponent<Mithrix.UpgradeMithrixBodyBehavior>();
                }
                else
                {
                    body.EnsureComponent<UpgradeHalcyoniteBodyBehavior>();
                }
            }
        }

        public class UpgradeHalcyoniteBodyBehavior : MonoBehaviour
        {
            private void Awake()
            {
                // TODO
            }
        }

        public class MithrixWithHalcyoniteBehavior : BossFightBehavior
        {
            protected override void OnBossSpawnedServer(CharacterBody body)
            {
                if (body.name.Contains("Brother"))
                {
                    if (body.TryGetComponent<CharacterDeathBehavior>(out var component))
                    {
                        component.deathState = new SerializableEntityStateType(typeof(TrueDeathState));
                    }
                }
                else if (body.Is(DLC2Content.BodyPrefabs.HalcyoniteBody))
                {
                    body.EnsureComponent<HalcyoniteBodyBehavior>();
                }
            }
        }
    }
}