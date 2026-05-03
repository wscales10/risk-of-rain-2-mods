using EntityStates;
using HG;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using System;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1
{
    public class Halcyonite1BodyBehavior : Stage1HalcyoniteBodyBehavior, IModifyOverlapAttack // TODO: do these classes this live on both client and server, or just server? Should there be checks for NetworkServer.active? Is using properties instead of fields okay? Should it be a NetworkBehavior? Same question applies to many of my behaviors.
    {
        public bool laserFirst;

        public CombatDirector? CombatDirector;

        private static float meleeAttackPushAwayForceMultiplier = 0.5f;

        public static void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.secondarySkill.bonusStockAdd += 2;
            args.attackSpeedTotalMult *= 0.5f;
            args.primarySkill.cooldownMultiplier *= 0.5f;
            args.specialSkill.cooldownMultiplier *= 4 / 3f;
            args.damageTotalMult *= 0.8f;
        }

        public void OnEnable()
        {
            RecalculateStats.Add(this.GetComponent<CharacterBody>(), OnRecalculateStats);
        }

        public void OnDisable()
        {
            RecalculateStats.Remove(this.GetComponent<CharacterBody>(), OnRecalculateStats);
        }

        public void ModifyOverlapAttack(BasicMeleeAttack state)
        {
            state.forceVector = Vector3.up * state.forceVector.y;
            state.pushAwayForce *= meleeAttackPushAwayForceMultiplier;

            if (state.overlapAttack != null)
            {
                state.overlapAttack.forceVector = Vector3.up * state.overlapAttack.forceVector.y;
                state.overlapAttack.pushAwayForce *= meleeAttackPushAwayForceMultiplier;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            this.Body.ScaleDifficultyAsBoss(new BossScalingArgs1(0.65f, 65f, false, 10), false); // TODO: rethink the way I'm scaling enemies, I need one or more helper methods which easily allow me to correctly scale enemy health, damage and most importantly, rewards. Also note that the combat squads scale enemy health for multiplayer by default, so at the moment I'm overscaling.
            Utils.ScaleDeathRewards(this.Body, Utils.CreditsForBossWave(10) / 200);
            this.EnsureComponent<HalcyoniteThrustBehavior>().getDesiredDistance = () => 16;
            this.Body.DisableStunsEtc();
        }

        protected override void SetupBossAi(BaseAI ai)
        {
            base.SetupBossAi(ai);

            ai.aimVectorMaxSpeed = 720f; // Turn twice as fast

            int index = Array.FindIndex(ai.skillDrivers, x => x.customName == "WhirlwindRush");

            if (index != -1)
            {
                CustomWeaponStates.CrossedFistsSkillState.customSkill.InsertSkillDriver(ai, index);
            }
        }
    }
}