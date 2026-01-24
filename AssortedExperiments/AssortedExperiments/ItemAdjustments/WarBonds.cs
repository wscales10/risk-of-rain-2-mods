using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace AssortedExperiments.ItemAdjustments
{
    public class WarBonds : Module
    {
        public override void Init()
        {
            IL.RoR2.BarrageOnBossBehaviour.FireMissile += BarrageOnBossBehaviour_FireMissile;
            On.RoR2.BarrageOnBossBehaviour.CalculateHitPosition += this.BarrageOnBossBehaviour_CalculateHitPosition;
            On.RoR2.BarrageOnBossBehaviour.UpdateBarrage += BarrageOnBossBehaviour_UpdateBarrage;
            On.RoR2.BarrageOnBossBehaviour.OnDisable += BarrageOnBossBehaviour_OnDisable;
        }

        private static void BarrageOnBossBehaviour_OnDisable(On.RoR2.BarrageOnBossBehaviour.orig_OnDisable orig, BarrageOnBossBehaviour self)
        {
            orig(self);

            if (self.TryGetComponent<BossBarrageContext>(out var context))
            {
                context.Position = null;
            }
        }

        private static void BarrageOnBossBehaviour_UpdateBarrage(On.RoR2.BarrageOnBossBehaviour.orig_UpdateBarrage orig, BarrageOnBossBehaviour self)
        {
            orig(self);

            if (self.bossmissileState == BarrageOnBossBehaviour.BossMissileState.None && self.TryGetComponent<BossBarrageContext>(out var context))
            {
                context.Position = null;
            }
        }

        private static void BarrageOnBossBehaviour_FireMissile(ILContext il)
        {
            var c = new ILCursor(il);

            // Don't set the target to self if it's null
            ILLabel? target = null;
            c.GotoNext(
                MoveType.AfterLabel,
                x => x.MatchLdloc(0),
                x => x.MatchCall<UnityEngine.Object>("op_Implicit"),
                x => x.MatchBrtrue(out target));
            c.RemoveRange(3);
            c.Emit(OpCodes.Br_S, target);

            while (c.TryGotoNext(
                x => x.MatchLdarg(0), // this
                x => x.MatchLdloc(0), // characterBody
                x => x.MatchGetVirt<Component>(nameof(Component.gameObject)), // characterBody.gameObject
                x => x.MatchCall<BarrageOnBossBehaviour>(nameof(BarrageOnBossBehaviour.CalculateHitPosition))) // this.CalculateHitPosition(characterBody.gameObject))
                )
            {
                c.Index += 2;
                c.Remove();
                c.EmitDelegate<Func<CharacterBody?, GameObject?>>((characterBody) => characterBody ? characterBody!.gameObject : null);
            }
        }

        private Vector3 BarrageOnBossBehaviour_CalculateHitPosition(On.RoR2.BarrageOnBossBehaviour.orig_CalculateHitPosition orig, BarrageOnBossBehaviour self, GameObject target)
        {
            (Vector3, float) ResolveSpreadOriginAndRadius()
            {
                var context = self.EnsureComponent<BossBarrageContext>();

                if (!target)
                {
                    if (context && context.Position.HasValue)
                    {
                        return (context.Position.Value, context.SpreadRadius);
                    }
                    else
                    {
                        target = self.gameObject;
                    }
                }

                var spreadOrigin = self.MoveTargetToGround(target.transform.position);

                this.Logger.LogDebug($"Setting War Bonds origin position to {spreadOrigin}");
                context.Position = spreadOrigin;

                float spreadRadius;
                if (self.isTargetBoss)
                {
                    spreadRadius = 1f;
                }
                else if (target == self.gameObject)
                {
                    spreadRadius = 10f;
                }
                else
                {
                    spreadRadius = 0f;
                }

                this.Logger.LogDebug($"Setting War Bonds spread radius to {spreadRadius}");
                context.SpreadRadius = spreadRadius;
                return (spreadOrigin, spreadRadius);
            }

            var (spreadOrigin, spreadRadius) = ResolveSpreadOriginAndRadius();
            Vector2 normalized = UnityEngine.Random.insideUnitCircle.normalized;
            return spreadOrigin + new Vector3(normalized.x * self.BarrageRadius * spreadRadius, 0f, normalized.y * self.BarrageRadius * spreadRadius);
        }
    }
}