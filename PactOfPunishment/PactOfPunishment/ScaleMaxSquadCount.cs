using MonoMod.Cil;
using RoR2;
using RoR2.Artifacts;
using System;
using UnityEngine.Networking;

namespace PactOfPunishment
{
    public class ScaleMaxSquadCount : Module
    {
        public delegate void ScaleMaxSquadCountDelegate(CombatDirector combatDirector, ref uint maxSquadCount);

        public static event ScaleMaxSquadCountDelegate? OnScaleMaxSquadCount;

        public override void Init()
        {
            IL.RoR2.CombatDirector.AttemptSpawnOnTarget += Utils.HookIL(this.CombatDirector_AttemptSpawnOnTarget);
            IL.RoR2.InfiniteTowerWaveController.FixedUpdate += Utils.HookIL(this.InfiniteTowerWaveController_FixedUpdate);
            On.RoR2.Artifacts.SwarmsArtifactManager.OnArtifactEnabled += this.SwarmsArtifactManager_OnArtifactEnabled;
            On.RoR2.Artifacts.SwarmsArtifactManager.OnArtifactDisabled += this.SwarmsArtifactManager_OnArtifactDisabled;
        }

        private void InfiniteTowerWaveController_FixedUpdate(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.AfterLabel, x => x.MatchLdfld<InfiniteTowerWaveController>(nameof(InfiniteTowerWaveController.maxSquadSize))))
            {
                c.Remove();
                c.EmitDelegate<Func<InfiniteTowerWaveController, int>>((self) =>
                {
                    uint maxSquadSize = (uint)self.maxSquadSize;
                    OnScaleMaxSquadCount?.Invoke(self.combatDirector, ref maxSquadSize);
                    return (int)maxSquadSize;
                });
            }
        }

        private static void ScaleMaxSquadCountForSwarmsArtifact(CombatDirector combatDirector, ref uint maxSquadCount)
        {
            maxSquadCount *= (uint)SwarmsArtifactManager.swarmSpawnCount;
        }

        private void SwarmsArtifactManager_OnArtifactDisabled(On.RoR2.Artifacts.SwarmsArtifactManager.orig_OnArtifactDisabled orig, RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            if (artifactDef == SwarmsArtifactManager.myArtifact)
            {
                OnScaleMaxSquadCount -= ScaleMaxSquadCountForSwarmsArtifact;
            }

            orig(runArtifactManager, artifactDef);
        }

        private void SwarmsArtifactManager_OnArtifactEnabled(On.RoR2.Artifacts.SwarmsArtifactManager.orig_OnArtifactEnabled orig, RunArtifactManager runArtifactManager, ArtifactDef artifactDef)
        {
            orig(runArtifactManager, artifactDef);

            if ((artifactDef == SwarmsArtifactManager.myArtifact) && NetworkServer.active)
            {
                OnScaleMaxSquadCount += ScaleMaxSquadCountForSwarmsArtifact;
            }
        }

        private void CombatDirector_AttemptSpawnOnTarget(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.AfterLabel, x => x.MatchLdfld<CombatDirector>(nameof(CombatDirector.maxSquadCount))))
            {
                c.Remove();
                c.EmitDelegate<Func<CombatDirector, uint>>(GetMaxSquadCount);
            }
        }

        public static uint GetMaxSquadCount(CombatDirector combatDirector)
        {
            var maxSquadCount = combatDirector.maxSquadCount;

            if (combatDirector.teamIndex != TeamIndex.Player)
            {
                OnScaleMaxSquadCount?.Invoke(combatDirector, ref maxSquadCount);
            }

            return maxSquadCount;
        }
    }
}