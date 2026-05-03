using EntityStates;
using RoR2.Skills;
using PactOfPunishment.Waves.Common;
using RoR2;
using System.Collections;
using UnityEngine;
using PactOfPunishment.Waves.Stage1.Halcyonites;

namespace PactOfPunishment.Waves.Halcyonites
{
    public static class HalcyoniteStates
    {
        public abstract class PhaseState<T> : EntityState, IOnBossTakeDamageReceiver
            where T : PhaseState<T>
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

            protected abstract T? GetNextPhaseState();

            private void TryAdvanceState()
            {
                if (this.healthComponent.combinedHealthFraction <= this.PhaseEndHealthThreshold)
                {
                    var nextPhaseState = this.GetNextPhaseState();

                    if (!(nextPhaseState == null))
                    {
                        this.outer.SetState(this.GetInterludeState(this.PhaseEndHealthThreshold, nextPhaseState));
                    }
                }
            }

            protected abstract InterludeState<T> GetInterludeState(float phaseEndHealthThreshold, T nextPhaseState);

            protected void OverrideSkill(SkillSlot skillSlot, SkillDef skillDef)
            {
                this.characterBody.skillLocator.GetSkill(skillSlot).SetSkillOverride(this.outer, skillDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public abstract class InterludeState<T> : EntityState
            where T : PhaseState<T>
        {
            private readonly T nextPhaseState;

            protected EntityStateMachine WeaponStateMachine { get; private set; }

            protected InterludeState(float phaseStartingHealthFraction, T nextPhaseState)
            {
                this.PhaseStartingHealthFraction = phaseStartingHealthFraction;
                this.nextPhaseState = nextPhaseState;
            }

            public float PhaseStartingHealthFraction { get; }

            protected abstract float Duration { get; }

            public override void OnEnter()
            {
                base.OnEnter();
                this.WeaponStateMachine = EntityStateMachine.FindByCustomName(this.characterBody.gameObject, "Weapon");
                this.SetupMainBossBody(this.characterBody);
                this.outer.StartCoroutine(this.Wait());
            }

            public override void Update()
            {
                base.Update();
                Utils.DirectHealIncludingShields(this.healthComponent, this.PhaseStartingHealthFraction);
            }

            public override void OnExit()
            {
                this.WeaponStateMachine.SetNextStateToMain();
                base.OnExit();
            }

            private IEnumerator Wait()
            {
                yield return new WaitForSeconds(this.Duration);
                this.outer.SetState(this.nextPhaseState);
            }

            private void SetupMainBossBody(CharacterBody? body)
            {
                if (body == null)
                {
                    return;
                }

                this.WeaponStateMachine.SetInterruptState(new CustomWeaponStates.ImmobileState(), InterruptPriority.Immobilize); // TODO: this might change for diferent states
            }
        }
    }
}
