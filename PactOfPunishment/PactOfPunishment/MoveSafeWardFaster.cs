using EntityStates.InfiniteTowerSafeWard;
using MonoMod.Cil;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public class MoveSafeWardFaster : Module
    {
        public override void Init()
        {
            IL.EntityStates.InfiniteTowerSafeWard.Travelling.FixedUpdate += Utils.HookIL(this.Travelling_FixedUpdate);
        }

        private static float GetTravelSpeedBonus(Travelling travellingState, float maxBonus, Vector3 playerPosition)
        {
            Vector3 vector = playerPosition - travellingState.zone.transform.position;
            vector.y = 0f;
            return Mathf.Max(0, maxBonus * (1 - vector.magnitude / travellingState.zone.radius));
        }

        private void Travelling_FixedUpdate(ILCursor c)
        {
            c.GotoNext(x => x.MatchLdfld<Travelling>(nameof(Travelling.travelSpeed)));
            c.Remove();
            c.EmitDelegate<Func<Travelling, float>>((self) =>
            {
                float? min = null;

                foreach (var x in PlayerCharacterMasterController.instances.Select(x => (x?.body)).Where(x => x?.healthComponent?.alive == true).Select(body => (body, body!.teamComponent?.transform?.position)).Where(x => x.position.HasValue))
                {
                    float bonus = GetTravelSpeedBonus(self, x.body!.moveSpeed - self.travelSpeed, x.position!.Value);

                    if (min is null || bonus < min)
                    {
                        min = bonus;
                    }
                }

                return self.travelSpeed + (min ?? 0);
            });
        }
    }
}