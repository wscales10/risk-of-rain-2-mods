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
        private void InitLunarRain()
        {
            // Base
            IL.EntityStates.FalseSonBoss.LunarRain.FireRain += Utils.HookIL(LunarRain_FireRain);

            // Upgraded
            On.EntityStates.FalseSonBoss.LunarRain.OnEnter += this.LunarRain_OnEnter;
        }


        private static void LunarRain_FireRain(ILCursor c)
        {
            c.GotoNext(MoveType.After,
                x => x.MatchLdsfld<LunarRain>(nameof(LunarRain.lunarRainPrefab)),
                x => x.MatchLdarg(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchCall<UnityEngine.Object>(nameof(UnityEngine.Object.Instantiate)),
                x => x.MatchCall<NetworkServer>(nameof(NetworkServer.Spawn))
            );
            c.Index--;
            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<GameObject, LunarRain>>((orig, self) =>
            {
                orig.GetComponent<DestroyOnTimer>().duration *= self.attackSpeedStat;
                orig.GetComponent<KillOnTimer>().duration *= self.attackSpeedStat;

                if (self.characterBody.TryGetComponent<FinalBossUpgradeStrategies.BodyBehavior>(out var behavior))
                {
                    behavior.OnSpawnLunarRaindrop(orig);
                }
            });
        }
        private void LunarRain_OnEnter(On.EntityStates.FalseSonBoss.LunarRain.orig_OnEnter orig, LunarRain self)
        {
            if (self.characterBody?.GetComponent<FinalBossUpgradeStrategies.BodyBehavior>())
            {
                float durationMultiplier = FinalBossUpgradeStrategies.LunarRainDurationMultiplier / self.attackSpeedStat;
                self.warningDuration *= durationMultiplier;
                self.duration *= durationMultiplier;
            }

            this.Logger.LogDebug($"Entering LunarRain state at {Run.instance.GetRunStopwatch()}");
            orig(self);
        }
    }
}
