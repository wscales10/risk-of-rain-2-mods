using HG;
using RoR2;
using RoR2.CharacterAI;
using System;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3
{
    public class Halcyonite3BossFightBehavior : Stage1HalcyoniteBossFightBehavior
    {
        public GameObject? DustCenterPrefab;

        protected override void SetupBossAi(BaseAI ai)
        {
            base.SetupBossAi(ai);

            ai.RemoveSkillDriversWhere(x => x.customName == "Golden Slash");

            int index = Array.FindIndex(ai.skillDrivers, x => x.customName == "WhirlwindRush");

            if (index != -1)
            {
                CustomWeaponStates.LineOfFistsSkillState.customSkill.InsertSkillDriver(ai, index);
            }
        }

        protected override void SetupThrustSkillDriver(AISkillDriver skillDriver)
        {
            base.SetupThrustSkillDriver(skillDriver);
            skillDriver.minDistance = 0;
        }

        protected override void SetupLaserSkillDriver(AISkillDriver skillDriver)
        {
            base.SetupLaserSkillDriver(skillDriver);
            skillDriver.minDistance = 0;
        }

        protected override void OnBossSpawnedServer(CharacterBody body)
        {
            if (body.Is(DLC2Content.BodyPrefabs.HalcyoniteBody))
            {
                body.ScaleDifficultyAsBoss(0.54f, 65f, true, false); // TODO: rethink the way I'm scaling enemies, I need one or more helper methods which easily allow me to correctly scale enemy health, damage and most importantly, rewards. Also note that the combat squads scale enemy health for multiplayer by default, so at the moment I'm overscaling.
                body.MakeUnscaledEliteUsingEquipment(DLC1Content.Elites.Earth);
                this.SetupBossAi(body);
                body.DisableStunsEtc();

                var halcyoniteBodyBehavior = body.EnsureComponent<Halcyonite3BodyBehavior>();
                halcyoniteBodyBehavior.DustCenterPrefab = this.DustCenterPrefab;
                halcyoniteBodyBehavior.rng = new Xoroshiro128Plus(this.CombatDirector.rng.nextUlong);
            }
        }
    }
}