using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using RoR2;
using UnityEngine;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves.Stage2
{
    public class WormAndDistributor : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private readonly AssetPromise<CharacterSpawnCard> wormSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Scorchling/cscScorchling.asset");

        private readonly AssetPromise<CharacterSpawnCard> distributorSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC3/MinePod/cscMinePod.asset");

        protected override UpgradeWaveStrategy GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<EnableSecondarySkillsStrategy>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = this.wormSpawnCard.Value,
                },
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = this.distributorSpawnCard.Value,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Close,
                }
            };

            dir.gameObject.AddComponent<WormAndDistributorBossFightBehavior>();
        }

        public class WormAndDistributorBossFightBehavior : BossFightBehavior
        {
            public bool disableSecondarySkills = true;

            public override void Awake()
            {
                base.Awake();
                this.EliminateCombatSquadWhenLastMainMemberDies(this.CombatDirector.combatSquad, x => x.GetBody().IsOneOf(DLC2Content.BodyPrefabs.ScorchlingBody, DLC3Content.BodyPrefabs.MinePodBody));
            }

            protected override void OnBossSpawnedServer(CharacterBody body)
            {
                if (body.Is(DLC2Content.BodyPrefabs.ScorchlingBody))
                {
                    body.ScaleDifficultyAsBoss(0.68f, 2.2f, true, false);

                    if (this.disableSecondarySkills)
                    {
                        Utils.DisableSkill(body, x => x.secondary);
                    }
                }
                else if (body.Is(DLC3Content.BodyPrefabs.MinePodBody))
                {
                    body.ScaleDifficultyAsBoss(0.2f, 100, true, false);
                    Utils.MakeUnscaledElite(body.inventory, RoR2Content.Elites.Ice);

                    if (this.disableSecondarySkills)
                    {
                        Utils.DisableSkill(body, x => x.secondary);
                    }
                }
            }
        }

        public class EnableSecondarySkillsStrategy : UpgradeWaveStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(InfiniteTowerWaveController wave)
            {
                var dir = wave.combatDirector;

                if (dir.TryGetComponent<WormAndDistributorBossFightBehavior>(out var behavior))
                {
                    behavior.disableSecondarySkills = false;
                }
            }
        }
    }
}