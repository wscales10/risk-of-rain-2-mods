using EntityStates.Halcyonite;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.BugFixes
{
    public class HalcyoniteGravity : Module
    {
        public override void Init()
        {
            On.EntityStates.Halcyonite.WhirlwindWarmUp.OnExit += this.WhirlwindWarmUp_OnExit;
            On.EntityStates.Halcyonite.WhirlWindPersuitCycle.OnEnter += this.WhirlWindPersuitCycle_OnEnter;
            On.EntityStates.EntityState.ModifyNextState += this.EntityState_ModifyNextState;
        }

        private void EntityState_ModifyNextState(On.EntityStates.EntityState.orig_ModifyNextState orig, EntityStates.EntityState self, EntityStates.EntityState nextState)
        {
            orig(self, nextState);

            // In a future update, this could result in animations/sounds being played twice. I think this is good enough though.
            if (self is WhirlwindWarmUp warmUpState && !(nextState is WhirlWindPersuitCycle))
            {
                warmUpState.characterMotor.velocity = Vector3.zero;
                warmUpState.PlayCrossfade("FullBody Override", "WhirlwindRushExit", "WhirlwindRush.playbackRate", 1f, 0.1f);
                Util.PlaySound("Play_halcyonite_skill3_end", warmUpState.gameObject);
                Util.PlaySound("Stop_halcyonite_skill3_loop", warmUpState.gameObject);
                warmUpState.characterBody.RecalculateStats();
            }
        }

        private void WhirlWindPersuitCycle_OnEnter(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_OnEnter orig, WhirlWindPersuitCycle self)
        {
            CharacterGravityParameters gravityParameters = self.characterMotor.gravityParameters;
            gravityParameters.channeledAntiGravityGranterCount++;

            CharacterFlightParameters flightParameters = self.characterMotor.flightParameters;
            flightParameters.channeledFlightGranterCount++;

            orig(self);

            self.characterMotor.gravityParameters = gravityParameters;
            self.characterMotor.flightParameters = flightParameters;
        }

        private void WhirlwindWarmUp_OnExit(On.EntityStates.Halcyonite.WhirlwindWarmUp.orig_OnExit orig, WhirlwindWarmUp self)
        {
            CharacterGravityParameters gravityParameters = self.characterMotor.gravityParameters;
            gravityParameters.channeledAntiGravityGranterCount--;

            CharacterFlightParameters flightParameters = self.characterMotor.flightParameters;
            flightParameters.channeledFlightGranterCount--;

            orig(self);

            self.characterMotor.gravityParameters = gravityParameters;
            self.characterMotor.flightParameters = flightParameters;
        }
    }
}