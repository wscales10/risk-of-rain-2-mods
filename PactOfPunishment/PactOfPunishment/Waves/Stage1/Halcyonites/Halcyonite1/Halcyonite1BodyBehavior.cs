using EntityStates;
using HG;
using PactOfPunishment.Waves.Halcyonites;
using R2API;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1
{
    public class Halcyonite1BodyBehavior : HalcyoniteBodyBehavior, IModifyOverlapAttack // TODO: do these classes this live on both client and server, or just server? Should there be checks for NetworkServer.active? Is using properties instead of fields okay? Should it be a NetworkBehavior? Same question applies to many of my behaviors.
    {
        public bool laserFirst;

        public CombatDirector? CombatDirector;

        private static float meleeAttackPushAwayForceMultiplier = 0.5f;

        public static void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
        {
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
            this.EnsureComponent<HalcyoniteThrustBehavior>().getDesiredDistance = () => 16;
            var stateMachine = this.gameObject.AddComponent<EntityStateMachine>();
            stateMachine.customName = "BossBody";
            this.Body?.healthComponent.ForwardBossDamageTo(stateMachine);
            stateMachine.SetState(new Halcyonite1States.Phase1());
        }
    }
}