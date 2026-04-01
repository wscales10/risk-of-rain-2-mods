using RoR2;

namespace PactOfPunishment
{
    public class DelayCombatDirectorStart : Module
    {
        public override void Init()
        {
            On.RoR2.CombatDirector.FixedUpdate += this.CombatDirector_FixedUpdate;
        }

        private void CombatDirector_FixedUpdate(On.RoR2.CombatDirector.orig_FixedUpdate orig, CombatDirector self)
        {
            if (self.TryGetComponent<CombatDirectorInitialDelay>(out var delayBehavior) && delayBehavior.Timer > 0)
            {
                return;
            }

            orig(self);
        }
    }
}