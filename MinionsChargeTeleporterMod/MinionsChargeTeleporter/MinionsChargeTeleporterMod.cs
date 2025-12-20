using BepInEx;
using BepInEx.Configuration;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MinionsChargeTeleporter
{
    [BepInPlugin("com.woodyscales.minionschargeteleporter", "Minions Charge Teleporter", "1.0.1")]
    public class MinionsChargeTeleporterMod : BaseUnityPlugin
    {
        private readonly Dictionary<HoldoutZoneController, HashSet<CharacterBody>> bodiesInZones = new Dictionary<HoldoutZoneController, HashSet<CharacterBody>>();

        private ConfigEntry<bool> prioritisePlayersWhenTeleporterNotActive;

        private bool prioritisePlayersWhenTeleporterNotActiveValue;

        public void Awake()
        {
            this.prioritisePlayersWhenTeleporterNotActive = this.Config.Bind("General", "Prioritise Players When Teleporter Not Active", true, "If true, outside of teleporter events enemies will be more likely to spawn near players than minions.");

            // Refresh setting value
            On.RoR2.SceneCatalog.OnActiveSceneChanged += this.SceneCatalog_OnActiveSceneChanged;

            // Allow non-players to charge holdout zones
            On.RoR2.HoldoutZoneController.CountPlayersInRadius += this.HoldoutZoneController_CountPlayersInRadius;

            // Stop teleporter charging text from flashing if there are enough allies in the zone
            On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.ShouldBeFlashing += ChargeHoldoutZoneObjectiveTracker_ShouldBeFlashing;

            // Stop certain minions from teleporting out of the holdout zone, even if their master
            // is far away
            IL.RoR2.Items.MinionLeashBodyBehavior.FixedUpdate += MinionLeashBodyBehavior_FixedUpdate;

            // Stop certain minions from following players out of the holdout zone
            On.RoR2.CharacterAI.BaseAI.EvaluateSingleSkillDriver += BaseAI_EvaluateSingleSkillDriver;

            // Monsters: prioritise targets in holdout zones for ambushes
            On.RoR2.CombatDirector.PickPlayerAsSpawnTarget += this.CombatDirector_PickPlayerAsSpawnTarget;

            // All AI: Prioritise targets in holdout zones for attacking
            IL.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += BaseAI_FindEnemyHurtBox;

            // Fix NRE in GetStolenInventoryInfo
            On.RoR2.ItemStealController.GetStolenInventoryInfo += ItemStealController_GetStolenInventoryInfo;
        }

        private static ItemStealController.StolenInventoryInfo ItemStealController_GetStolenInventoryInfo(On.RoR2.ItemStealController.orig_GetStolenInventoryInfo orig, ItemStealController self, Inventory victimInventory)
        {
            return self.stolenInventoryInfos?.FirstOrDefault(x => x.victimInventory == victimInventory);
        }

        private static bool ChargeHoldoutZoneObjectiveTracker_ShouldBeFlashing(On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.orig_ShouldBeFlashing orig, HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker self)
        {
            var teamIndex = self.sourceDescriptor.master.teamIndex;
            var allTeamMembers = TeamComponent.GetTeamMembers(teamIndex);
            var numberOfChargingPlayersAndMinions = allTeamMembers.Count(teamComponent => self.holdoutZoneController.IsBodyInChargingRadius(teamComponent.body));
            return numberOfChargingPlayersAndMinions < allTeamMembers.Count(teamComponent => teamComponent.body.isPlayerControlled);
        }

        private static void BaseAI_FindEnemyHurtBox(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(x => x.MatchLdarg(0), x => x.MatchLdfld("RoR2.CharacterAI.BaseAI", "enemySearch"), x => x.MatchCallvirt("RoR2.BullseyeSearch", "GetResults"));
            c.Index += 3;
            c.GotoNext();
            c.RemoveRange(2);
            c.EmitDelegate<Func<IEnumerable<HurtBox>, IEnumerable<HurtBox>>>(FastSort);
        }

        private static IEnumerable<HurtBox> FastSort(IEnumerable<HurtBox> input)
        {
            Utils.SetHoldoutZones();
            return input.OrderByDescending(h => Utils.IsInHoldoutZone(h?.transform?.position)).ToArray();
        }

        private static RoR2.CharacterAI.BaseAI.SkillDriverEvaluation? BaseAI_EvaluateSingleSkillDriver(On.RoR2.CharacterAI.BaseAI.orig_EvaluateSingleSkillDriver orig, RoR2.CharacterAI.BaseAI self, ref RoR2.CharacterAI.BaseAI.SkillDriverEvaluation currentSkillDriverEvaluation, RoR2.CharacterAI.AISkillDriver aiSkillDriver, float myHealthFraction)
        {
            if (Utils.WantsToStayInZone(self.master) && aiSkillDriver.moveTargetType == RoR2.CharacterAI.AISkillDriver.TargetType.CurrentLeader)
            {
                return null;
            }

            return orig(self, ref currentSkillDriverEvaluation, aiSkillDriver, myHealthFraction);
        }

        private static void MinionLeashBodyBehavior_FixedUpdate(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchCall("RoR2.Items.BaseItemBodyBehavior", "get_body"),
                x => x.MatchCallvirt("RoR2.CharacterBody", "get_master"),
                x => x.MatchStloc(0));
            c.Index += 3;
            c.EmitDelegate<Func<CharacterMaster, CharacterMaster>>(master => Utils.WantsToStayInZone(master) ? null : master);
        }

        private void SceneCatalog_OnActiveSceneChanged(On.RoR2.SceneCatalog.orig_OnActiveSceneChanged orig, UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
        {
            this.prioritisePlayersWhenTeleporterNotActiveValue = this.prioritisePlayersWhenTeleporterNotActive.Value;
            orig(oldScene, newScene);
        }

        private int HoldoutZoneController_CountPlayersInRadius(On.RoR2.HoldoutZoneController.orig_CountPlayersInRadius orig, HoldoutZoneController holdoutZoneController, Vector3 origin, float chargingRadiusSqr, TeamIndex teamIndex)
        {
            if (teamIndex != TeamIndex.Player)
            {
                return orig(holdoutZoneController, origin, chargingRadiusSqr, teamIndex);
            }

            return this.CountPlayersAndAlliesInRadius(holdoutZoneController, teamIndex);
        }

        private int CountPlayersAndAlliesInRadius(HoldoutZoneController holdoutZoneController, TeamIndex teamIndex)
        {
            var newSet = Utils.GetBodiesInHoldoutZone(holdoutZoneController, teamIndex);

            if (!this.bodiesInZones.TryGetValue(holdoutZoneController, out var oldSet))
            {
                oldSet = new HashSet<CharacterBody>();
            }

            this.bodiesInZones[holdoutZoneController] = newSet;
            var added = newSet.Except(oldSet).ToArray();
            var removed = oldSet.Except(newSet).ToArray();

            foreach (var body in added)
            {
                this.Logger.LogDebug($"{body?.GetDisplayName()} entered zone '{holdoutZoneController?.name}', following AIs {Utils.GetListOfAIsRunningBody(body)}.");
            }

            foreach (var body in removed)
            {
                this.Logger.LogDebug($"{body?.GetDisplayName()} left zone '{holdoutZoneController?.name}', following AIs {Utils.GetListOfAIsRunningBody(body)}.");
            }

            var output = newSet.Count;
            output = Math.Min(output, Utils.GetTotalNumberOfPlayers(teamIndex));
            return output;
        }

        private void CombatDirector_PickPlayerAsSpawnTarget(On.RoR2.CombatDirector.orig_PickPlayerAsSpawnTarget orig, CombatDirector self)
        {
            Utils.SetHoldoutZones();

            bool isEnemy(TeamIndex teamIndex)
            {
                return teamIndex != TeamIndex.Neutral && teamIndex != self.teamIndex;
            }

            var instances = CharacterMaster.instancesList.Where(master => isEnemy(master.teamIndex) && master.hasBody);
            var list1 = new List<CharacterMaster>();
            var list2 = new List<CharacterMaster>();
            var list3 = new List<CharacterMaster>();

            foreach (var master in instances)
            {
                if (Utils.IsInHoldoutZone(master))
                {
                    list1.Add(master);
                }
                else if (master.playerCharacterMasterController || Utils.IsAnyHoldoutZoneActive || !this.prioritisePlayersWhenTeleporterNotActiveValue)
                {
                    list2.Add(master);
                }
                else
                {
                    list3.Add(master);
                }
            }

            if (list1.Count > 0)
            {
                self.currentSpawnTarget = self.rng.NextElementUniform(list1).GetBodyObject();
            }
            else if (list2.Count > 0)
            {
                self.currentSpawnTarget = self.rng.NextElementUniform(list2).GetBodyObject();
            }
            else if (list3.Count > 0)
            {
                self.currentSpawnTarget = self.rng.NextElementUniform(list3).GetBodyObject();
            }
        }
    }
}