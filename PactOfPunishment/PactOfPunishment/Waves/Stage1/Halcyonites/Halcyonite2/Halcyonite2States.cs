using PactOfPunishment.Waves.Halcyonites;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2
{
    public static class Halcyonite2States
    {
        public class Phase1 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0.75f;

            public override void OnEnter()
            {
                base.OnEnter();
                this.BodyBehavior.CanUseNewSkill = false;
                this.BodyBehavior.SpawnPillars = false;
                this.BodyBehavior.powerMeter.Persistent = false;
            }

            protected override PhaseState? GetNextPhaseState() => new Phase2();
        }

        public class Phase2 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0.5f;

            public override void OnEnter()
            {
                base.OnEnter();
                this.BodyBehavior.CanUseNewSkill = true;
                this.BodyBehavior.SpawnPillars = false;
                this.BodyBehavior.powerMeter.Persistent = false;
            }

            protected override PhaseState? GetNextPhaseState() => new Phase3();
        }

        public class Phase3 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0.25f;

            public override void OnEnter()
            {
                base.OnEnter();
                this.BodyBehavior.CanUseNewSkill = true;
                this.BodyBehavior.SpawnPillars = true;
                this.BodyBehavior.powerMeter.Persistent = false;
            }

            protected override PhaseState? GetNextPhaseState() => new Phase4();
        }

        public class Phase4 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0;

            public override void OnEnter()
            {
                base.OnEnter();
                this.BodyBehavior.CanUseNewSkill = true;
                this.BodyBehavior.SpawnPillars = true;
                this.BodyBehavior.powerMeter.Persistent = true;
            }

            protected override PhaseState? GetNextPhaseState() => null;
        }

        public abstract class PhaseState : HalcyoniteStates.PhaseState<PhaseState>
        {
            protected Halcyonite2BodyBehavior BodyBehavior { get; private set; }

            public override void OnEnter()
            {
                base.OnEnter();
                this.BodyBehavior = this.GetComponent<Halcyonite2BodyBehavior>();
            }

            protected override HalcyoniteStates.InterludeState<PhaseState> GetInterludeState(float phaseEndHealthThreshold, PhaseState nextPhaseState)
            {
                return new InterludeState(phaseEndHealthThreshold, nextPhaseState);
            }
        }

        public class InterludeState : HalcyoniteStates.InterludeState<PhaseState>
        {
            private readonly bool isLast;

            public InterludeState(float phaseStartingHealthFraction, PhaseState nextPhaseState) : base(phaseStartingHealthFraction, nextPhaseState)
            {
                this.isLast = Mathf.Approximately(nextPhaseState.PhaseEndHealthThreshold, 0);
            }

            protected override float Duration => 3;

            public override void OnEnter()
            {
                base.OnEnter();
                FastTrackCombatDirectorCredits(this.GetComponent<Halcyonite2BodyBehavior>().CombatDirector);

                if (this.isLast)
                {
                    this.GetComponent<Halcyonite2BodyBehavior>().powerMeter.Persistent = true;
                }
            }

            private static void FastTrackCombatDirectorCredits(CombatDirector? combatDirector)
            {
                if (!combatDirector)
                {
                    return;
                }

                var monsterCard = combatDirector!.lastAttemptedMonsterCard;

                if (combatDirector.monsterCredit < monsterCard?.cost)
                {
                    combatDirector.monsterCredit = monsterCard.cost;
                }
            }
        }
    }
}
