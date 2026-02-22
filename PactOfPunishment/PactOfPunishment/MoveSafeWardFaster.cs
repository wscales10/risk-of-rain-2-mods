using EntityStates.InfiniteTowerSafeWard;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace PactOfPunishment
{
    public class MoveSafeWardFaster : Module
    {
        public override void Init()
        {
            IL.EntityStates.InfiniteTowerSafeWard.Travelling.FixedUpdate += Utils.HookIL(this.Travelling_FixedUpdate);
        }

        private void Travelling_FixedUpdate(ILCursor c)
        {
            c.GotoNext(x => x.MatchLdfld<Travelling>(nameof(Travelling.travelSpeed)));
            c.Remove();
            c.EmitDelegate<Func<Travelling, float>>((self) =>
            {
                return self.travelSpeed * (1 + PlayerCharacterMasterController.instances.Select(x => x?.body).Where(x => x?.healthComponent?.alive == true).Select(x => x!.teamComponent?.transform?.position).Where(x => x.HasValue).Min(x => GetTravelSpeedBonus(self, x!.Value)));
            });
        }

        private static float GetTravelSpeedBonus(Travelling travellingState, Vector3 playerPosition)
        {
            Vector3 vector = playerPosition - travellingState.zone.transform.position;
            vector.y = 0f;
            return Mathf.Max(0, 1 * (1 - vector.magnitude / travellingState.zone.radius));
        }
    }
}