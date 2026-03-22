using RoR2;
using System;

namespace PactOfPunishment.ProtectMonstersFromHazards
{
    public class ReduceFogDamage : Module
    {
        public override void Init()
        {
            /* Hades lava is just worse than Simulacrum Void Fog in general.
             * I decided to add this module because of a time when I went slightly the wrong way (the focus went up and I didn't).
             * I think I might have even been able to wait for myself to regenerate (assuming no lasting consequences), but I didn't.
             * Maybe I can use NVidia to record my next death from void fog to see if it actually needs tweaking (using RoR2 data rather than Hades lava info)?
             */

            // On.RoR2.InfiniteTowerRun.Start += this.InfiniteTowerRun_Start;
        }

        private void InfiniteTowerRun_Start(On.RoR2.InfiniteTowerRun.orig_Start orig, InfiniteTowerRun self)
        {
            try
            {
                FogDamageController fogDamageController = self.fogDamagePrefab.GetComponent<FogDamageController>();
                fogDamageController.healthFractionPerSecond = 0.1f;
                fogDamageController.healthFractionRampIncreaseCooldown = 0.4f;
                fogDamageController.healthFractionRampCoefficientPerSecond = 5f;
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex);
            }
            finally
            {
                orig(self);
            }
        }
    }
}
