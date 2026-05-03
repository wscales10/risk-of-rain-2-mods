using HG;
using PactOfPunishment.AiSkillDrivers;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using RoR2.CharacterAI;
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

            foreach (var skillDriver in body.GetSkillDrivers(SkillSlot.Utility))
            {
                // Enable utility skill immediately
                skillDriver.maxUserHealthFraction = float.PositiveInfinity;
            }

            /*
            foreach (var skillDriver in body.master.GetSkillDrivers("FireAndChase"))
            {
                skillDriver.minDistance = 15;
            }

            var newSkillDriver = body.master.gameObject.AddComponent<AISkillDriver>();
            SetupStrafeSkillDriver(newSkillDriver);
            body.master.GetComponent<BaseAI>().AddSkillDriver(newSkillDriver);
            */
            body.EnsureComponent<UndeployMinionsOnDeathBehavior>();
            body.EnsureComponent<SolusControlUnitBodyBehavior>();
        }

        private static void SetupStrafeSkillDriver(AISkillDriver skillDriver)
        {
            skillDriver.customName = "Strafe";
            skillDriver.skillSlot = SkillSlot.None;
            skillDriver.minDistance = 10;
            skillDriver.maxDistance = 35;
            skillDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skillDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skillDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            skillDriver.ignoreNodeGraph = true;
        }

        private static void SetupFleeSkillDriver(AISkillDriver skillDriver)
        {
            skillDriver.customName = "Strafe";
            skillDriver.skillSlot = SkillSlot.None;
            skillDriver.minDistance = 10;
            skillDriver.maxDistance = 35;
            skillDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            skillDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            skillDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            skillDriver.ignoreNodeGraph = true;
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