using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Infrastructure;
using R2API;
using RoR2;
using System;
using UnityEngine;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves.Stage1
{
    public partial class SolusControlUnitMiniBossInfo : PortableMiniBossInfo<SolusControlUnitBossFightBehavior>
    {
        private static readonly Lazy<DeployableSlot> deployableSlot = new Lazy<DeployableSlot>(() => DeployableAPI.RegisterDeployableSlot((_, __) => int.MaxValue));

        private readonly AssetPromise<CharacterSpawnCard> solusControlUnitSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/RoboBallBoss/cscRoboBallBoss.asset");

        public override SpawnInfo SpawnInfo => new SpawnInfo
        {
            count = 1,
            spawnCard = this.solusControlUnitSpawnCard.Value, // TODO: can fail to spawn in Abyssal Depths?
            spawnDistance = Content.MonsterSpawnDistances.WithinZone,
        };

        public override void SetupBossBody(CharacterBody body, SolusControlUnitBossFightBehavior bossFightBehavior)
        {
            if (bossFightBehavior.disableSpecialSkill)
            {
                this.DisableSkill(body, SkillSlot.Special);
                this.DisableSkill(body, SkillSlot.Primary);
            }

            body.EnsureComponent<UndeployMinionsOnDeathBehavior>();
            body.EnsureComponent<SolusControlUnitBodyBehavior>();
           
            SimulacrumWavesModule.OnTakeNonZeroDamageGlobal += (victim, _) => OnTakeNonZeroDamageGlobal(victim, body);
        }

        private static void OnTakeNonZeroDamageGlobal(HealthComponent victim, CharacterBody solusControlUnitBody)
        {
            if (victim == solusControlUnitBody.healthComponent)
            {
                solusControlUnitBody.GetComponent<RateLimiter>().TryDoThing();
            }
        }
    }

    public class SolusControlUnitBossFightBehavior : PortableMiniBossFightBehavior<SolusControlUnitBossFightBehavior>
    {
        public bool disableSpecialSkill = true;
    }

    public sealed class SolusControlUnit : PortableMiniBossWaveDefinition<SolusControlUnitBossFightBehavior>
    {
        public SolusControlUnit() : base(ScriptableObject.CreateInstance<SolusControlUnitMiniBossInfo>())
        {
        }

        protected override PickupDropTable GetRewardDropTable(Run run)
        {
            return BetterExplicitPickupDropTable.ReplaceTierWithSingleItem(GetBaseDropTable(run), RoR2Content.Items.RoboBallBuddy);
        }

        protected override UpgradeEncounterStrategy GetUpgradeStrategy() => ScriptableObject.CreateInstance<EnableSpecialSkillUpgradeStrategy>();

        public class EnableSpecialSkillUpgradeStrategy : UpgradeEncounterStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(EncounterContext ctx)
            {
                var dir = ctx.CombatDirector;

                if (dir.TryGetComponent<SolusControlUnitBossFightBehavior>(out var behavior))
                {
                    behavior.disableSpecialSkill = false;
                }
            }
        }
    }
}