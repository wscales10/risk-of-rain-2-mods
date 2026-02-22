using EntityStates;
using PactOfPunishment.Waves.Common;
using RoR2;
using System.Collections;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    public static class Halcyonite1States
    {
        public abstract class BaseState : EntityState
        {
        }

        public class Phase1 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0.75f;

            protected override PhaseState? GetNextPhaseState() => new Phase2();
        }

        public class Phase2 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0.5f;

            public override void OnEnter()
            {
                base.OnEnter();

                if (!this.GetComponent<Halcyonite1.Halcyonite1BodyBehavior>().laserFirst)
                {
                    this.OverrideUtilitySkill();
                }
                else
                {
                    foreach (var ai in this.characterBody.master.AiComponents)
                    {
                        Halcyonite1.SetLaserBehaviorEnabled(ai, true);
                    }
                }
            }

            protected override PhaseState? GetNextPhaseState() => new Phase3();
        }

        public class Phase3 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0.25f;

            protected override PhaseState? GetNextPhaseState() => new Phase4();
        }

        public class Phase4 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0;

            public override void OnEnter()
            {
                base.OnEnter();

                if (this.GetComponent<Halcyonite1.Halcyonite1BodyBehavior>().laserFirst)
                {
                    this.OverrideUtilitySkill();
                }
                else
                {
                    foreach (var ai in this.characterBody.master.AiComponents)
                    {
                        Halcyonite1.SetLaserBehaviorEnabled(ai, true);
                    }
                }
            }

            protected override PhaseState? GetNextPhaseState() => null;
        }

        public abstract class PhaseState : BaseState, IOnBossTakeDamageReceiver
        {
            public abstract float PhaseEndHealthThreshold { get; }

            void IOnBossTakeDamageReceiver.OnBossDamageTaken() => this.TryAdvanceState();

            public override void OnEnter()
            {
                base.OnEnter();

                if (this.PhaseEndHealthThreshold > 0)
                {
                    Utils.MakeBodySemiImmortal(this.characterBody);
                }
                else
                {
                    Utils.MakeBodyMortal(this.characterBody);
                }
            }

            protected abstract PhaseState? GetNextPhaseState();

            protected void OverrideUtilitySkill()
            {
                this.characterBody.skillLocator.utility.SetSkillOverride(this, HalcyoniteModule.UntitledSkillState.skillDef, GenericSkill.SkillOverridePriority.Contextual);
            }

            private void TryAdvanceState()
            {
                if (this.healthComponent.combinedHealthFraction <= this.PhaseEndHealthThreshold)
                {
                    var phaseState = this.GetNextPhaseState();

                    if (!(phaseState is null))
                    {
                        this.outer.SetState(new InterludeState(this.PhaseEndHealthThreshold, phaseState));
                    }
                }
            }
        }

        public class InterludeState : BaseState
        {
            private readonly PhaseState nextPhaseState;

            private EntityStateMachine weaponStateMachine;

            public InterludeState(float phaseStartingHealthFraction, PhaseState nextPhaseState)
            {
                this.PhaseStartingHealthFraction = phaseStartingHealthFraction;
                this.nextPhaseState = nextPhaseState;
            }

            public float PhaseStartingHealthFraction { get; }

            public override void OnEnter()
            {
                base.OnEnter();
                this.weaponStateMachine = EntityStateMachine.FindByCustomName(this.characterBody.gameObject, "Weapon");
                this.SetupMainBossBody(this.characterBody);
                this.outer.StartCoroutine(this.Wait());
            }

            public override void Update()
            {
                base.Update();
                Utils.DirectHeal(this.healthComponent, this.PhaseStartingHealthFraction);
            }

            public override void OnExit()
            {
                this.weaponStateMachine.SetNextStateToMain();
                base.OnExit();
            }

            private IEnumerator Wait()
            {
                yield return new WaitForSeconds(4);
                this.outer.SetState(this.nextPhaseState);
            }

            private void SetupMainBossBody(CharacterBody? body)
            {
                if (body is null)
                {
                    return;
                }

                this.weaponStateMachine.SetInterruptState(new HalcyoniteModule.ImmobileState(), InterruptPriority.Immobilize);
            }
        }
    }
}