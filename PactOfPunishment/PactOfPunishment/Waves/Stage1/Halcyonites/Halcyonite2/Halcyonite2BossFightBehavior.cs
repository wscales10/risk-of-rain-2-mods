using HG;
using PactOfPunishment.Waves.Halcyonites;
using RoR2;
using RoR2.CharacterAI;
using System;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2
{
    public class Halcyonite2BossFightBehavior : HalcyoniteBossFightBehavior
    {
        public override void Awake()
        {
            base.Awake();
            this.gameObject.EliminateCombatSquadWhenLastMainMemberDies(this.CombatDirector.combatSquad, x => x.GetBody()?.bodyIndex == DLC2Content.BodyPrefabs.HalcyoniteBody.bodyIndex, callback: () =>
            {
                this.CombatDirector.enabled = false;

                foreach (var pillar in FindObjectsOfType<PillarMovementBehavior>())
                {
                    Destroy(pillar.gameObject); // I guess I could filter by team or something, but I don't think it's necessary
                }
            });
        }

        protected override void OnBossSpawnedServer(CharacterBody body)
        {
            if (body.Is(DLC2Content.BodyPrefabs.HalcyoniteBody))
            {
                body.ScaleDifficultyAsBoss(0.62f, 65f, true, false);

                this.SetupBossAi(body);
                body.DisableStunsEtc();

                var halcyoniteBodyBehavior = body.EnsureComponent<Halcyonite2BodyBehavior>();
                halcyoniteBodyBehavior.CombatDirector = this.CombatDirector;
            }
            else if (body.Is(DLC3Content.BodyPrefabs.WorkerUnitBody))
            {
                body.ScaleDifficultyAsBoss(1f, 30f, true, false); // TODO: coef was kinda factored into spawning
            }
        }

        protected override void SetupBossAi(BaseAI ai)
        {
            base.SetupBossAi(ai);

            CustomWeaponStates.RepeatingFistSkillState.customSkill.InsertSkillDriver(ai, Array.FindIndex(ai.skillDrivers, sd => sd.skillSlot == SkillSlot.Primary) + 1);

            foreach (var skillDriver in ai.GetSkillDrivers("Follow Target"))
            {
                skillDriver.shouldSprint = true;
            }
        }

        protected override void SetupLaserSkillDriver(AISkillDriver skillDriver)
        {
            base.SetupLaserSkillDriver(skillDriver);
            skillDriver.movementType = AISkillDriver.MovementType.Stop;
            skillDriver.moveInputScale = 0;
            skillDriver.requiredSkill = HalcyoniteModule.LaserSkillDef.Value;
            skillDriver.minDistance = 0;
        }
    }
}