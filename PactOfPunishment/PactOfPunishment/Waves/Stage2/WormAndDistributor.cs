using HG;
using PactOfPunishment.AiSkillDrivers;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using UnityEngine;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves.Stage2
{
    public class WormAndDistributor : PortableMiniBossWaveDefinition<WormAndDistributorBossFightBehavior>
    {
        public WormAndDistributor() : base(ScriptableObject.CreateInstance<WormMiniBossInfo>(), ScriptableObject.CreateInstance<DistributorMiniBossInfo>())
        {
        }

        protected override UpgradeEncounterStrategy GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<EnableSecondarySkillsStrategy>();
        }

        public class WormMiniBossInfo : PortableMiniBossInfo<WormAndDistributorBossFightBehavior>
        {
            private readonly AssetPromise<CharacterSpawnCard> wormSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Scorchling/cscScorchling.asset");

            public override SpawnInfo SpawnInfo => new SpawnInfo
            {
                count = 1,
                spawnCard = this.wormSpawnCard.Value,
            };

            public override void SetupBossBody(CharacterBody body, WormAndDistributorBossFightBehavior bossFightBehavior)
            {
                body.ScaleDifficultyAsBoss(new BossScalingArgs1(0.68f, 2.2f, false, 15), false);
                Utils.ScaleDeathRewards(body, Utils.CreditsForBossWave(15) * 0.5f / 80);

                foreach (var ai in body.master.AiComponents)
                {
                    ai.xrayVision = true;

                    foreach (var skillDriver in ai.GetSkillDrivers("Breach"))
                    {
                        // Extend breach distance
                        skillDriver.maxDistance = 5;
                    }

                    foreach (var skillDriver in ai.GetSkillDrivers("ChaseOffNodegraph"))
                    {
                        // Sprint more
                        skillDriver.shouldSprint = true;
                    }
                }

                if (bossFightBehavior.disableSecondarySkills)
                {
                    this.DisableSkill(body, SkillSlot.Secondary);
                }

                body.EnsureComponent<WormBossBodyBehavior>();
            }

            public class WormBossBodyBehavior : MonoBehaviour
            {
            }
        }

        public class DistributorMiniBossInfo : PortableMiniBossInfo<WormAndDistributorBossFightBehavior>
        {
            private readonly AssetPromise<CharacterSpawnCard> distributorSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC3/MinePod/cscMinePod.asset");

            public override SpawnInfo SpawnInfo => new SpawnInfo
            {
                count = 1,
                spawnCard = this.distributorSpawnCard.Value,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Close,
            };

            public override void SetupBossBody(CharacterBody body, WormAndDistributorBossFightBehavior bossFightBehavior)
            {
                body.ScaleDifficultyAsBoss(new BossScalingArgs1(0.2f, 100, false, 15), false);
                Utils.ScaleDeathRewards(body, Utils.CreditsForBossWave(15) * 0.5f / 20);
                Utils.MakeUnscaledEliteUsingEquipment(body, RoR2Content.Elites.Ice);

                foreach (var skillDriver in body.GetSkillDrivers("PlantMine"))
                {
                    skillDriver.maxDistance = 140;
                }

                if (bossFightBehavior.disableSecondarySkills)
                {
                    this.DisableSkill(body, SkillSlot.Secondary);
                }

                body.EnsureComponent<DistributorBossBodyBehavior>();
            }

            public class DistributorBossBodyBehavior : BossBodyBehavior
            {
                public static float mineTriggerRadius = 12f;

                public void OnEnable()
                {
                    RecalculateStats.Add(this.Body, this.OnRecalculateStats);
                }

                public void OnDisable()
                {
                    RecalculateStats.Remove(this.Body, this.OnRecalculateStats);
                }

                private void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
                {
                    args.primarySkill.cooldownMultiplier *= 0.5f;
                }
            }
        }

        public class EnableSecondarySkillsStrategy : UpgradeEncounterStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(EncounterContext ctx)
            {
                var dir = ctx.CombatDirector;

                if (dir.TryGetComponent<WormAndDistributorBossFightBehavior>(out var behavior))
                {
                    behavior.disableSecondarySkills = false;
                }
            }
        }
    }
}