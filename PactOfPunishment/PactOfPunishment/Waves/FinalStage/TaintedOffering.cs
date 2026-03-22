using EntityStates;
using EntityStates.FalseSonBoss;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace PactOfPunishment.Waves.FinalStage
{
    public partial class FinalBoss : Module
    {
        private void InitTaintedOffering()
        {
            On.EntityStates.FalseSonBoss.TaintedOffering.ModifyProjectileAimRay += TaintedOffering_ModifyProjectileAimRay;
            On.EntityStates.FalseSonBoss.TaintedOffering.OnExit += this.TaintedOffering_OnExit;
        }

        private void TaintedOffering_OnExit(On.EntityStates.FalseSonBoss.TaintedOffering.orig_OnExit orig, TaintedOffering self)
        {
            orig(self);

            // There's no point in checking what the coefficient was on entry to this state - you want to reset it to 1.
            // Perhaps instead I should set it to 1 on entry to some other state. Oh well.
            self.characterMotor.walkSpeedPenaltyCoefficient = 1f;
        }

        private static Ray TaintedOffering_ModifyProjectileAimRay(On.EntityStates.FalseSonBoss.TaintedOffering.orig_ModifyProjectileAimRay orig, TaintedOffering self, Ray aimRay)
        {
            // TODO: use an IL hook instead?
            aimRay.origin = self.modelLocator.modelTransform.GetComponent<ChildLocator>().FindChild("MuzzleRight").position;
            return aimRay;
        }
    }
}
