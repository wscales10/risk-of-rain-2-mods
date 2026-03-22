using HG;
using PactOfPunishment.Waves.Halcyonites;
using R2API;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3
{
    public static class Halcyonite3States
    {
        public class Phase1 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 2 / 3f;

            protected override PhaseState? GetNextPhaseState() => new Phase2();
        }

        public class Phase2 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 1 / 3f;

            public override void OnEnter()
            {
                base.OnEnter();
                this.bodyBehavior.isBurstLaserEnabled = true;
            }

            protected override PhaseState? GetNextPhaseState() => new Phase3();
        }

        public class Phase3 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0;

            public override void OnEnter()
            {
                base.OnEnter();
                this.bodyBehavior.isThreeWayFistsEnabled = true;
            }

            protected override PhaseState? GetNextPhaseState() => null;
        }

        public abstract class PhaseState : HalcyoniteStates.PhaseState<PhaseState>
        {
            protected Halcyonite3BodyBehavior bodyBehavior { get; private set; }

            public override void OnEnter()
            {
                base.OnEnter();
                this.bodyBehavior = this.GetComponent<Halcyonite3BodyBehavior>();
                this.OverrideSkill(SkillSlot.Special, CustomWeaponStates.LineOfFistsSkillState.customSkill.SkillDef);
            }

            protected override HalcyoniteStates.InterludeState<PhaseState> GetInterludeState(float phaseEndHealthThreshold, PhaseState nextPhaseState)
            {
                return new InterludeState(phaseEndHealthThreshold, nextPhaseState, 1 - 0.75f * (1 - phaseEndHealthThreshold));
            }
        }

        public class InterludeState : HalcyoniteStates.InterludeState<PhaseState>
        {
            private readonly float desiredSafeZoneRadiusPercentage;

            private float? startingSafeZoneRadiusPercentage;

            private SafeZoneRadiusCapper? radiusCapper;

            private bool hasStartedBlink = false;

            public InterludeState(float phaseStartingHealthFraction, PhaseState nextPhaseState, float desiredSafeZoneRadiusPercentage) : base(phaseStartingHealthFraction, nextPhaseState)
            {
                this.desiredSafeZoneRadiusPercentage = desiredSafeZoneRadiusPercentage;
            }

            protected override float Duration => 4.6f;

            public override void OnEnter()
            {
                base.OnEnter();

                if (Run.instance is InfiniteTowerRun run && run.waveController)
                {
                    this.startingSafeZoneRadiusPercentage = run.waveController.zoneRadiusPercentage;
                    this.radiusCapper = run.waveController.EnsureComponent<SafeZoneRadiusCapper>();
                }
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();

                if (this.fixedAge > this.Duration / 6f && !this.hasStartedBlink)
                {
                    this.hasStartedBlink = true;

                    var blinkState = Utils.InstantiateState<EntityStates.ImpBossMonster.BlinkState>();
                    blinkState.exitDuration = (this.Duration - this.fixedAge) / 5f;
                    blinkState.duration = blinkState.exitDuration * 4;

                    this.WeaponStateMachine.SetNextState(blinkState);
                }

                if (this.radiusCapper)
                {
                    this.radiusCapper!.MaximumRadiusPercentage = Mathf.Min(this.radiusCapper.MaximumRadiusPercentage, Mathf.Lerp(this.startingSafeZoneRadiusPercentage!.Value, this.desiredSafeZoneRadiusPercentage, 1.5f * this.fixedAge / this.Duration - 0.25f));
                }
            }
        }
    }
}