using BepInEx;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using R2API;
using R2API.Utils;
using Mono.Cecil.Cil;

namespace RoR2Mod1
{
	[BepInDependency("com.bepis.r2api")]
	[BepInPlugin("com.woodyscales.testmod1", "My First Mod", "1.0.0")]
	[BepInDependency(R2API.R2API.PluginGUID)]
	[R2APISubmoduleDependency(nameof(ItemAPI))]
	public class MyFirstMod : BaseUnityPlugin
	{
		public void Awake()
		{
			Logger.LogMessage("Loaded Woody's Mod!");

			// Prioritise targets in holdout zones for monster ambushes
			On.RoR2.CombatDirector.PickPlayerAsSpawnTarget += CombatDirector_PickPlayerAsSpawnTarget;

			// Allow non-players to charge holdout zones
			IL.RoR2.HoldoutZoneController.FixedUpdate += HoldoutZoneController_FixedUpdate;

			// Replace MinionLeash with MinionLeash2
			IL.RoR2.MinionOwnership.MinionGroup.AddMinion += MinionGroup_AddMinion;

			// Stop certain minions from following players out of the holdout zone
			On.RoR2.CharacterAI.BaseAI.EvaluateSingleSkillDriver += BaseAI_EvaluateSingleSkillDriver;

			// Prioritise targets in holdout zones for attacking
			IL.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += BaseAI_FindEnemyHurtBox;
		}

		private static void BaseAI_FindEnemyHurtBox(ILContext il)
		{
			var c = new ILCursor(il);
			c.GotoNext(x => x.MatchLdarg(0), x => x.MatchLdfld("RoR2.CharacterAI.BaseAI", "enemySearch"), x => x.MatchCallvirt("RoR2.BullseyeSearch", "GetResults"));
			c.GotoNext();
			c.RemoveRange(2);
			c.EmitDelegate<Func<IEnumerable<HurtBox>, HurtBox>>(x => FastSort(x).FirstOrDefault());
			c.Emit(OpCodes.Ret);
		}

		private void MinionGroup_AddMinion(ILContext il)
		{
			var c = new ILCursor(il);
			c.GotoNext(x => x.MatchLdsfld("RoR2.RoR2Content/Items", "MinionLeash"));
			c.Remove();
			c.Emit(OpCodes.Call, typeof(Items).GetMethod("get_MinionLeash2"));
		}

		private static void HoldoutZoneController_FixedUpdate(ILContext il)
		{
			var c = new ILCursor(il);
			c.Index += 15;
			// c.GotoNext(x => x.MatchCallvirt("HoldoutZoneController", "CountPlayersInRadius"));
			c.Remove();
			c.EmitDelegate<Func<HoldoutZoneController, Vector3, float, TeamIndex, int>>(CountPlayersAndAlliesInRadius);
		}

		private static int CountPlayersAndAlliesInRadius(HoldoutZoneController holdoutZoneController, Vector3 origin, float chargingRadiusSqr, TeamIndex teamIndex)
		{
			var output = TeamComponent.GetTeamMembers(teamIndex).Count(teamComponent => holdoutZoneController.IsBodyInChargingRadius(teamComponent.body));
			output = Math.Min(output, TeamComponent.GetTeamMembers(teamIndex).Count(teamComponent => teamComponent.body.isPlayerControlled));
			return output;
		}

		private static IEnumerable<HurtBox> FastSort(IEnumerable<HurtBox> input)
		{
			Utils.SetHoldoutZones();
			return input.Where(h => Utils.IsInHoldoutZone(h?.transform?.position));
		}

		private RoR2.CharacterAI.BaseAI.SkillDriverEvaluation? BaseAI_EvaluateSingleSkillDriver(On.RoR2.CharacterAI.BaseAI.orig_EvaluateSingleSkillDriver orig, RoR2.CharacterAI.BaseAI self, ref RoR2.CharacterAI.BaseAI.SkillDriverEvaluation currentSkillDriverEvaluation, RoR2.CharacterAI.AISkillDriver aiSkillDriver, float myHealthFraction)
		{
			if (Utils.WantsToStayInZone(self.master) && aiSkillDriver.moveTargetType == RoR2.CharacterAI.AISkillDriver.TargetType.CurrentLeader)
			{
				return null;
			}

			return orig(self, ref currentSkillDriverEvaluation, aiSkillDriver, myHealthFraction);
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

			foreach (var master in instances)
			{
				if (Utils.IsInHoldoutZone(master))
				{
					list1.Add(master);
				}
				else
				{
					list2.Add(master);
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
		}
	}
}