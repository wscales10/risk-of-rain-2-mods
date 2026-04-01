using EntityStates;
using PactOfPunishment.Waves.Halcyonites;
using RoR2;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1
{
    public static class Halcyonite1States
    {
        public class Support : EntityState
        {
            public override void OnEnter()
            {
                base.OnEnter();
                var bodyBehavior = this.GetComponent<Halcyonite1BodyBehavior>();
                bodyBehavior.EnableSkill(this.characterBody, SkillSlot.Secondary);
            }
        }

        public class Phase1 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0.75f;

            public override void OnEnter()
            {
                base.OnEnter();
                this.bodyBehavior.DisableSkill(this.characterBody, SkillSlot.Secondary); // TODO: assumes that we always enter phase 1 first, and never revisit previous phases. That's probably fine though.
            }

            protected override PhaseState? GetNextPhaseState() => new Phase2();
        }

        public class Phase2 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0.5f;

            public override void OnEnter()
            {
                base.OnEnter();

                if (!this.GetComponent<Halcyonite1BodyBehavior>().laserFirst)
                {
                    this.OverrideSkill();
                }
                else
                {
                    this.bodyBehavior.EnableSkill(this.characterBody, SkillSlot.Secondary);
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

                if (this.GetComponent<Halcyonite1BodyBehavior>().laserFirst)
                {
                    this.OverrideSkill();
                }
                else
                {
                    this.bodyBehavior.EnableSkill(this.characterBody, SkillSlot.Secondary);
                }
            }

            protected override PhaseState? GetNextPhaseState() => null;
        }

        public abstract class PhaseState : HalcyoniteStates.PhaseState<PhaseState>
        {
            protected Halcyonite1BodyBehavior bodyBehavior { get; private set; }

            public override void OnEnter()
            {
                base.OnEnter();
                this.bodyBehavior = this.GetComponent<Halcyonite1BodyBehavior>();
            }

            protected void OverrideSkill() => this.OverrideSkill(SkillSlot.Utility, CustomWeaponStates.CrossedFistsSkillState.customSkill.SkillDef);

            protected override HalcyoniteStates.InterludeState<PhaseState> GetInterludeState(float phaseEndHealthThreshold, PhaseState nextPhaseState)
            {
                return new InterludeState(phaseEndHealthThreshold, nextPhaseState);
            }
        }

        public class InterludeState : HalcyoniteStates.InterludeState<PhaseState>
        {
            public InterludeState(float phaseStartingHealthFraction, PhaseState nextPhaseState) : base(phaseStartingHealthFraction, nextPhaseState)
            {
            }

            protected override float Duration => 4;

            public override void OnEnter()
            {
                base.OnEnter();
                Utils.FastTrackCombatDirectorCredits(this.GetComponent<Halcyonite1BodyBehavior>().CombatDirector);
            }
        }
    }
}