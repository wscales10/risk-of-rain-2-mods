using RoR2;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AssortedExperiments
{
    public static class Utils
    {
        public static bool HasEquipment(Inventory inventory, EquipmentIndex equipmentIndex)
        {
            return inventory._equipmentStateSlots.SelectMany(x => x).Any(v => v.equipmentIndex == equipmentIndex);
        }

        public static bool IsScrapper(DirectorCard? directorCard)
        {
            return directorCard?.spawnCard?.name?.Contains("Scrapper") == true;
        }

        public static PlayerCharacterMasterController? GetAttackingPlayerFromDamageReportInternal(DamageReport damageReport)
        {
            return damageReport.attackerMaster?.playerCharacterMasterController ?? damageReport.attackerOwnerMaster?.playerCharacterMasterController;
        }

        public static string GetTimeString()
        {
            var run = Run.instance;

            if (run is null)
            {
                return "no run";
            }

            return $"Stage {run.stageClearCount + 1} / {TimeSpan.FromSeconds(run.GetRunStopwatch()):mm\\:ss\\.ff} / coef {run.difficultyCoefficient}";
        }

        public static string GetSceneDisplayName(Scene scene)
        {
            try
            {
                SceneDef sceneDef = SceneCatalog.GetSceneDefFromScene(scene);
                return Language.GetString(sceneDef.nameToken);
            }
            catch
            {
                return "??";
            }
        }

        public static void ServerFunctionCalledOnClient(this object self, [CallerMemberName] string? functionName = null)
        {
            Debug.LogWarning($"[Server] function '{self.GetType()}.{functionName}' called on client");
        }

        public static string GetBodyDisplayName(CharacterBody summonedBody)
        {
            try
            {
                return Language.GetString(summonedBody.baseNameToken);
            }
            catch
            {
                return "??";
            }
        }

        public static bool Is(this CharacterBody? body, CharacterBody? other)
        {
            if (body)
            {
                return other && body!.bodyIndex == other!.bodyIndex;
            }
            else
            {
                return !other;
            }
        }

        public static bool IsBossHealthBelowThreshold(BossGroup bossGroup, float normalizedPercent)
        {
            if (bossGroup == null || bossGroup.combatSquad.memberCount <= 0)
            {
                return false;
            }

            return bossGroup.totalObservedHealth / Mathf.Max(bossGroup.totalMaxObservedMaxHealth, 1f) <= normalizedPercent;
        }
    }
}