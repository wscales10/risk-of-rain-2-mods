using EntityStates;
using EntityStates.FalseSonBoss;
using MonoMod.Cil;
using UnityEngine;

namespace PactOfPunishment.Waves.FinalStage
{
    // TODO: separate modules for bugfixes and adjustments
    public partial class FinalBoss : Module
    {
        private static void TaintedOffering_FixedUpdate(ILCursor c)
        {
            c.InterceptLoadField<GenericProjectileBaseState, float>(nameof(GenericProjectileBaseState.baseDuration), self => self.duration);
        }

        private static Ray TaintedOffering_ModifyProjectileAimRay(On.EntityStates.FalseSonBoss.TaintedOffering.orig_ModifyProjectileAimRay orig, TaintedOffering self, Ray aimRay)
        {
            // TODO: use an IL hook instead?
            aimRay.origin = self.modelLocator.modelTransform.GetComponent<ChildLocator>().FindChild("MuzzleRight").position;
            return aimRay;
        }

        private void InitTaintedOffering()
        {
            On.EntityStates.FalseSonBoss.TaintedOffering.OnEnter += this.TaintedOffering_OnEnter;
            IL.EntityStates.FalseSonBoss.TaintedOffering.FixedUpdate += Utils.HookIL(TaintedOffering_FixedUpdate);
            On.EntityStates.FalseSonBoss.TaintedOffering.ModifyProjectileAimRay += TaintedOffering_ModifyProjectileAimRay;
            On.EntityStates.FalseSonBoss.TaintedOffering.OnExit += this.TaintedOffering_OnExit;
        }

        private void TaintedOffering_OnEnter(On.EntityStates.FalseSonBoss.TaintedOffering.orig_OnEnter orig, TaintedOffering self)
        {
            var durationMultiplier = 1.2f;
            self.baseDuration *= durationMultiplier;
            self.baseDelayBeforeFiringProjectile *= durationMultiplier;
            orig(self);
        }

        private void TaintedOffering_OnExit(On.EntityStates.FalseSonBoss.TaintedOffering.orig_OnExit orig, TaintedOffering self)
        {
            orig(self);

            // There's no point in checking what the coefficient was on entry to this state - you
            // want to reset it to 1. Perhaps instead I should set it to 1 on entry to some other
            // state. Oh well.
            self.characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }
    }
}