using PactOfPunishment.Waves.Halcyonites;

namespace PactOfPunishment.Waves.Stage3
{
    public static class FinalHalcyoniteStates
    {
        public class Phase1 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0.5f;

            protected override PhaseState? GetNextPhaseState() => new Phase2();

            public override void OnExit()
            {
                this.bodyBehavior.DesiredState = FinalHalcyoniteBodyBehavior.State.Collective;
                base.OnExit();
            }
        }

        public class Phase2 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0;

            public override void OnEnter()
            {
                base.OnEnter();
                this.bodyBehavior.DesiredState = FinalHalcyoniteBodyBehavior.State.CollectivePlus;
            }

            protected override PhaseState? GetNextPhaseState() => null;
        }

        public abstract class PhaseState : HalcyoniteStates.PhaseState<PhaseState>
        {
            protected FinalHalcyoniteBodyBehavior bodyBehavior { get; private set; }

            public override void OnEnter()
            {
                base.OnEnter();
                this.bodyBehavior = this.GetComponent<FinalHalcyoniteBodyBehavior>();
            }

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

            protected override float Duration => 4; // TODO
        }
    }
}