using EntityStates.Halcyonite;
using HG;
using PactOfPunishment.Navigation;
using PactOfPunishment.Waves.Common;
using RoR2;

namespace PactOfPunishment.Waves.Halcyonites
{
    public class WhirlWindNavigationController : BossBodyBehavior
    {
        private FrustrationMonitor frustrationMonitor;

        private EntityStateMachine? weaponStateMachine;

        private WhirlWindModule.UseAirNodesController useAirNodesController;

        private bool IsWhirlWindActive => this.weaponStateMachine?.state is WhirlWindPersuitCycle whirlWindState && whirlWindState.state == WhirlWindPersuitCycle.PersuitState.Dash;

        protected override void Awake()
        {
            this.frustrationMonitor = new FrustrationMonitor(() => this.IsWhirlWindActive);
            this.frustrationMonitor.IsBuildingFrustrationChanged += this.OnFrustrationChanged;
            base.Awake();
            this.weaponStateMachine = EntityStateMachine.FindByCustomName(this.Body.gameObject, "Weapon");
            this.useAirNodesController = this.EnsureComponent<WhirlWindModule.UseAirNodesController>();
        }

        protected override void ManagedFixedUpdate(float deltaTime)
        {
            // TODO: over whole mod, make dash speed configurable rather than static
            float expectedSpeed = WhirlWindPersuitCycle.dashSpeedCoefficient;
            float actualSpeed = this.Body.rigidbody.velocity.magnitude;
            this.frustrationMonitor.Update(expectedSpeed, actualSpeed, deltaTime);
        }

        private void OnFrustrationChanged(bool value)
        {
            if (value)
            {
                this.useAirNodesController.Deactivate();
            }
            else
            {
                this.useAirNodesController.Activate();
            }
        }
    }
}