using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage2
{
    public class ScorchWormModule : Module
    {
        public override void Init()
        {
            On.EntityStates.Scorchling.ScorchlingBreach.OnEnter += this.ScorchlingBreach_OnEnter;
            IL.ScorchlingController.Breach += Utils.HookIL(ScorchlingController_Breach);
        }

        private static void ScorchlingController_Breach(ILCursor c)
        {
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<ScorchlingController>(nameof(ScorchlingController.lavaBombSkill)),
                x => x.MatchLdcI4(1),
                x => x.MatchCallvirt<GenericSkill>($"set_{nameof(GenericSkill.stock)}")
            );

            c.Index += 2;
            c.RemoveRange(2);
            c.EmitDelegate<Action<GenericSkill>>(lavaBombSkill =>
            {
                if (lavaBombSkill.characterBody?.GetComponent<WormAndDistributor.WormMiniBossInfo.WormBossBodyBehavior>())
                {
                    // Do nothing, I don't want the worm boss using its lava bomb too often
                }
                else
                {
                    // Don't reduce the stock to 1 if the worm has backup mag(s)
                    lavaBombSkill.stock = Mathf.Max(lavaBombSkill.stock, 1);
                }
            });
        }

        private void ScorchlingBreach_OnEnter(On.EntityStates.Scorchling.ScorchlingBreach.orig_OnEnter orig, EntityStates.Scorchling.ScorchlingBreach self)
        {
            if (self.GetComponent<WormAndDistributor.WormMiniBossInfo.WormBossBodyBehavior>())
            {
                self.crackToBreachTime *= 0.75f;
                self.breachToBurrow *= 0.5f;
            }

            orig(self);
        }
    }
}