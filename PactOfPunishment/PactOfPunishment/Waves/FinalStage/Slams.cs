using EntityStates;
using EntityStates.FalseSonBoss;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using System.Reflection;
using UnityEngine;

namespace PactOfPunishment.Waves.FinalStage
{
    public partial class FinalBoss : Module
    {
        private SkillDef swatAwayPlayersSkillDef;

        public static float GetPreFissureSlamDuration(CharacterBody body)
        {
            return Utils.InstantiateState<FissureSlamWindup>().baseChargeDuration / body.attackSpeed + Utils.InstantiateState<FissureSlam>().baseDuration / body.attackSpeed;
        }

        private static void FissureSlamWindup_FixedUpdate(ILCursor c)
        {
            c.GotoNext(
                x => x.MatchCall<FissureSlamWindup>($"get_{nameof(FissureSlamWindup.charge)}"),
                x => x.MatchCall<Time>($"get_{nameof(Time.deltaTime)}"),
                x => x.MatchAdd(),
                x => x.MatchCall<FissureSlamWindup>($"set_{nameof(FissureSlamWindup.charge)}"));
            c.Index++;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldfld, typeof(BaseState).GetField(nameof(BaseState.attackSpeedStat), BindingFlags.NonPublic | BindingFlags.Instance));
            c.Emit(OpCodes.Mul);
        }

        private void InitSlams()
        {
            On.EntityStates.FalseSonBoss.FissureSlam.HasLoSToPlayer += this.FissureSlam_HasLoSToPlayer;
            On.EntityStates.FalseSonBoss.SwatAwayPlayersSlam.HasLoSToPlayer += this.SwatAwayPlayersSlam_HasLoSToPlayer;
            On.EntityStates.FalseSonBoss.FissureSlam.DetonateAuthority += this.FissureSlam_DetonateAuthority;
            Utils.OnLoad<SkillDef>("RoR2/DLC2/FalseSonBoss/FalseSonBossSwatAwayPlayers.asset", x => this.swatAwayPlayersSkillDef = x);
            IL.EntityStates.FalseSonBoss.FissureSlamWindup.FixedUpdate += Utils.HookIL(FissureSlamWindup_FixedUpdate);
            On.RoR2.CharacterAI.BaseAI.GameObjectPassesSkillDriverFilters += this.BaseAI_GameObjectPassesSkillDriverFilters;
        }

        private bool BaseAI_GameObjectPassesSkillDriverFilters(On.RoR2.CharacterAI.BaseAI.orig_GameObjectPassesSkillDriverFilters orig, RoR2.CharacterAI.BaseAI self, RoR2.CharacterAI.BaseAI.Target target, RoR2.CharacterAI.AISkillDriver skillDriver, out float separationSqrMagnitude)
        {
            if (!orig(self, target, skillDriver, out separationSqrMagnitude))
            {
                return false;
            }

            if (IsFalseSonBoss(self.body) && skillDriver.skillSlot == SkillSlot.Primary)
            {
                var output = IsNoObstacleDirectlyAhead(self.body);
                this.Logger.LogDebug($"{(output ? "Allowing" : "Disallowing")} {skillDriver.customName} after raycast.");
                return output;
            }

            return true;
        }

        private static bool IsNoObstacleDirectlyAhead(CharacterBody body)
        {
            return !Physics.Raycast(body.corePosition, body.characterDirection.forward, 11, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
        }

        private bool SwatAwayPlayersSlam_HasLoSToPlayer(On.EntityStates.FalseSonBoss.SwatAwayPlayersSlam.orig_HasLoSToPlayer orig, SwatAwayPlayersSlam self)
        {
            return true;
        }

        private BlastAttack.Result FissureSlam_DetonateAuthority(On.EntityStates.FalseSonBoss.FissureSlam.orig_DetonateAuthority orig, FissureSlam self)
        {
            var result = orig(self);
            var explosionCentre = self.FindModelChild("ClubExplosionPoint").transform.position;
            new BlastAttack
            {
                attacker = self.gameObject,
                baseDamage = 0,
                baseForce = FissureSlam.blastForce * 5,
                bonusForce = FissureSlam.blastBonusForce,
                canRejectForce = false,
                crit = false,
                falloffModel = BlastAttack.FalloffModel.QuarterLinear,
                procCoefficient = 0,
                radius = FissureSlam.blastRadius + 25f,
                position = 0.25f * explosionCentre + 0.75f * self.characterBody.footPosition - 15 * Vector3.up,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                teamIndex = self.teamComponent.teamIndex,
            }.Fire();

            var slamSound = FalseSonBossGenericStateWithSwing.impactSound;
            if (slamSound && slamSound.index != NetworkSoundEventIndex.Invalid)
            {
                PointSoundManager.EmitSoundLocal(NetworkSoundEventCatalog.GetAkIdFromNetworkSoundEventIndex(slamSound.index), explosionCentre);
            }

            return result;
        }

        private bool FissureSlam_HasLoSToPlayer(On.EntityStates.FalseSonBoss.FissureSlam.orig_HasLoSToPlayer orig, FissureSlam self)
        {
            // TODO: would be better to use an IL hook to not even call this method, but oh well
            // TODO: maybe check LOS to club end position to check that the club won't phase through a wall
            return true;
        }

        private void Primary_onSkillChanged(GenericSkill obj)
        {
            foreach (var ai in obj.characterBody.master.AiComponents)
            {
                ai.skillDriverUpdateTimer = 0;
            }
        }
    }
}