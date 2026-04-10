using EntityStates;
using EntityStates.Halcyonite;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Stage1.Halcyonites;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3;
using PactOfPunishment.Waves.Stage3;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Halcyonites
{
    public class HalcyoniteModule : Module
    {
        public static SkillDef WhirlwindSkillDef;

        private static AssetPromise<SkillDef>? laserSkillDef;

        public static AssetPromise<SkillDef> LaserSkillDef => laserSkillDef ??= Utils.BeginLoad<SkillDef>("RoR2/DLC2/Halcyonite/HalcyoniteMonsterTriLaser.asset");

        public static float GetAttackSpeedStat(BaseState self)
        {
            var value = self.attackSpeedStat;

            if (self is TriLaser && self.TryGetComponent<Halcyonite3BodyBehavior>(out var halcyonite3BodyBehavior) && halcyonite3BodyBehavior.CurrentLaserMode == Halcyonite3BodyBehavior.LaserMode.Disrupt)
            {
                value *= 3;
            }

            if (self.GetComponent<Halcyonite1BodyBehavior>())
            {
                value = Mathf.Max(1, value);
            }

            return value;
        }

        public override void Init()
        {
            // Start loading laser skill def
            _ = LaserSkillDef;

            // Modify thrust attack
            On.EntityStates.Halcyonite.GoldenSwipe.PlayAnimation += GoldenSwipe_PlayAnimation;
            IL.EntityStates.BasicMeleeAttack.AuthorityOnFinish += Utils.HookIL(BasicMeleeAttack_AuthorityOnFinish);
            On.EntityStates.BasicMeleeAttack.OnExit += this.BasicMeleeAttack_OnExit;
            IL.EntityStates.BasicMeleeAttack.AuthorityFixedUpdate += Utils.HookIL(BasicMeleeAttack_AuthorityFixedUpdate);
            On.RoR2.CharacterAI.BaseAI.UpdateBodyAim += this.BaseAI_UpdateBodyAim;

            // Modify slash attack
            On.EntityStates.Halcyonite.GoldenSlash.PlayAnimation += this.GoldenSlash_PlayAnimation;

            // Modify laser attack
            IL.EntityStates.Halcyonite.ChargeTriLaser.OnEnter += Utils.HookIL(ModifyLaserAttackSpeed);
            IL.EntityStates.Halcyonite.TriLaser.OnEnter += Utils.HookIL(ModifyLaserAttackSpeed);
            IL.EntityStates.Halcyonite.TriLaser.FireTriLaser += Utils.HookIL(ModifyLaserAttackSpeed);
            IL.EntityStates.Halcyonite.TriLaser.FireTriLaser += Utils.HookIL(MakeDisruptLaserStun);
            IL.EntityStates.Halcyonite.TriLaser.FixedUpdate += Utils.HookIL(TriLaser_FixedUpdate);
            FinalHalcyoniteBodyBehavior.SetupSkillDef();

            Content.DamageTypes.Stun1sBypassImmunity = DamageAPI.ReserveDamageType();
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += this.SetStateOnHurt_OnTakeDamageServer;

            // Modify whirlwind attack
            On.EntityStates.Halcyonite.WhirlWindPersuitCycle.OnExit += WhirlWindPersuitCycle_OnExit;
            On.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateLand += this.WhirlWindPersuitCycle_UpdateLand;

            On.EntityStates.Halcyonite.WhirlWindPersuitCycle.GetMinimumInterruptPriority += this.WhirlWindPersuitCycle_GetMinimumInterruptPriority;
            WhirlWindPersuitCycle.maxSearchDist *= 4;

            Utils.OnLoad<SkillDef>("RoR2/DLC2/Halcyonite/HalcyoniteMonsterWhirlwindRush.asset", x => WhirlwindSkillDef = x);
            CustomWeaponStates.Init(this.Logger);

            // Allow disabling child teleport (should probably be moved to its own module)
            On.RoR2.ChildMonsterController.CheckTeleportAvailable += ChildMonsterController_CheckTeleportAvailable;

            IL.RoR2.Skills.SkillDef.CanExecute += Utils.HookIL(OverrideSkillInterruptPriority);
            IL.RoR2.Skills.SkillDef.OnExecute += Utils.HookIL(OverrideSkillInterruptPriority);

            On.RoR2.GenericSkill.Awake += this.GenericSkill_Awake;

            Utils.OnLoad<GameObject>("RoR2/Base/Brother/BrotherFirePillar.prefab", x =>
            {
                var pillarPrefab = PrefabAPI.InstantiateClone(x, "Halcyonite2PillarPrefab");
                pillarPrefab.AddComponent<PillarMovementBehavior>();
                Halcyonite2BodyBehavior.PillarPrefab = pillarPrefab;
            });
        }

        private static void OverrideSkillInterruptPriority(ILCursor c)
        {
            c.GotoNext(x => x.MatchLdfld<SkillDef>(nameof(SkillDef.interruptPriority)));
            c.Remove();
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Func<SkillDef, GenericSkill, InterruptPriority>>((self, skill) =>
            {
                if (skill.characterBody.TryGetComponent<FinalHalcyoniteBodyBehavior>(out var behavior))
                {
                    return behavior.GetSkillInterruptPriority(self);
                }

                return self.interruptPriority;
            });
        }

        private static void BasicMeleeAttack_AuthorityFixedUpdate(ILCursor c)
        {
            ILLabel? retLabel = null;
            c.GotoLast(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<BasicMeleeAttack>(nameof(BasicMeleeAttack.duration)),
                x => x.MatchLdarg(0),
                x => x.MatchCall<EntityState>($"get_{nameof(EntityState.fixedAge)}"),
                x => x.MatchBgtUn(out retLabel));
            c.Index--;
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, float, BasicMeleeAttack, bool>>((duration, fixedAge, self) =>
            {
                if (fixedAge < duration)
                {
                    return false;
                }

                if (self is GoldenSwipe && self.characterBody.TryGetComponent<Halcyonite3BodyBehavior>(out var behavior) && behavior.CurrentThrustContext == Halcyonite3BodyBehavior.ThrustContext.PostLaser && fixedAge < duration * 2)
                {
                    return false;
                }

                return true;
            });
            c.Emit(OpCodes.Brfalse_S, retLabel);
        }

        private static void MakeDisruptLaserStun(ILCursor c)
        {
            c.GotoNext(x => x.MatchCallvirt<BlastAttack>(nameof(BlastAttack.Fire)));
            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<BlastAttack, TriLaser>>((blastAttack, self) =>
            {
                if (self.characterBody.TryGetComponent<Halcyonite3BodyBehavior>(out var behavior) && behavior.CurrentLaserMode == Halcyonite3BodyBehavior.LaserMode.Disrupt)
                {
                    blastAttack.AddModdedDamageType(Content.DamageTypes.Stun1sBypassImmunity);
                }
            });
        }

        private static void BasicMeleeAttack_AuthorityOnFinish(ILCursor c)
        {
            c.GotoNext(x => x.MatchCallvirt<EntityStateMachine>(nameof(EntityStateMachine.SetNextStateToMain)));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EntityStateMachine, BasicMeleeAttack>>((outer, self) =>
            {
                if (self is GoldenSwipe && self.characterBody.TryGetComponent<Halcyonite3BodyBehavior>(out var behavior))
                {
                    switch (behavior.CurrentPostThrustState)
                    {
                        case Halcyonite3BodyBehavior.PostThrustState.Thrust:
                            outer.SetNextState(EntityStateCatalog.InstantiateState(typeof(GoldenSwipe)));
                            return;

                        case Halcyonite3BodyBehavior.PostThrustState.Slash:
                            outer.SetNextState(EntityStateCatalog.InstantiateState(typeof(GoldenSlash)));
                            return;
                    }
                }

                outer.SetNextStateToMain();
            });
        }

        private static void TriLaser_FixedUpdate(ILCursor c)
        {
            c.GotoNext(x => x.MatchCallvirt<EntityStateMachine>(nameof(EntityStateMachine.SetNextStateToMain)));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EntityStateMachine, TriLaser>>((outer, self) =>
            {
                if (self.characterBody.TryGetComponent<Halcyonite3BodyBehavior>(out var behavior) && behavior.CurrentLaserMode == Halcyonite3BodyBehavior.LaserMode.Disrupt)
                {
                    outer.SetNextState(EntityStateCatalog.InstantiateState(typeof(GoldenSwipe)));
                }
                else
                {
                    outer.SetNextStateToMain();
                }
            });
        }

        private static void WhirlWindPersuitCycle_UpdateDash(ILCursor c)
        {
            ILLabel? label = null;
            c.GotoNext(x => x.MatchLdarg(0),
                x => x.MatchLdfld<WhirlWindPersuitCycle>(nameof(WhirlWindPersuitCycle.dashSafeOutTime)),
                x => x.MatchLdsfld<WhirlWindPersuitCycle>(nameof(WhirlWindPersuitCycle.dashSafeExitDuration)),
                x => x.MatchBleUn(out label));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<WhirlWindPersuitCycle, bool>>(self => FallRiskMitigator.IsInDangerOfFalling(self.characterBody));
            c.Emit(OpCodes.Brtrue_S, label);
        }

        private static bool ChildMonsterController_CheckTeleportAvailable(On.RoR2.ChildMonsterController.orig_CheckTeleportAvailable orig, ChildMonsterController self)
        {
            if (self.GetComponent<DisableChildMonsterTeleport>()?.enabled == true)
            {
                return false;
            }

            return orig(self);
        }

        private static void WhirlWindPersuitCycle_OnExit(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_OnExit orig, WhirlWindPersuitCycle self)
        {
            Debug.Log($"Exiting state EntityStates.Halcyonite.WhirlWindPersuitCycle at {Run.instance?.fixedTime.ToString() ?? "??"} seconds.");
            orig(self);
        }

        private static void LogOnSetWhirlWindPersuitStateIL(ILCursor c)
        {
            c.GotoNext(MoveType.AfterLabel, x => x.MatchStfld<WhirlWindPersuitCycle>(nameof(WhirlWindPersuitCycle.state)));
            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<WhirlWindPersuitCycle.PersuitState>>(value =>
            {
                Debug.Log($"Setting EntityStates.Halcyonite.WhirlWindPersuitCycle.state to PersuitState.{value} at {Run.instance?.fixedTime.ToString() ?? "??"} seconds.");
            });
        }

        private static void ModifyLaserAttackSpeed(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.AfterLabel, x => x.MatchLdfld<BaseState>(nameof(BaseState.attackSpeedStat))))
            {
                c.Remove();
                c.EmitDelegate<Func<BaseState, float>>(GetAttackSpeedStat);
            }
        }

        private static void GoldenSwipe_PlayAnimation(On.EntityStates.Halcyonite.GoldenSwipe.orig_PlayAnimation orig, GoldenSwipe self)
        {
            orig(self);

            if (self.TryGetComponent<IModifyOverlapAttack>(out var overlapAttackModifier))
            {
                overlapAttackModifier.ModifyOverlapAttack(self);
            }

            if (self.TryGetComponent<HalcyoniteThrustBehavior>(out var halcyoniteThrustBehavior))
            {
                if (self.GetComponent<Halcyonite3BodyBehavior>())
                {
                    var targetPosition = self.characterBody.master.GetComponents<BaseAI>().FirstOrDefault(x => x)?.currentEnemy?.lastKnownBullseyePosition;

                    halcyoniteThrustBehavior.getDesiredDistance = () =>
                    {
                        float desiredDistance = targetPosition is null ? 0 : Vector3.Project(targetPosition.Value - self.characterBody.footPosition, self.characterBody.GetHorizontalFacingDirection()).magnitude;
                        return Mathf.Max(30, desiredDistance);
                    };
                }

                halcyoniteThrustBehavior.OnSwipe(self);
            }
        }

        private static void WhirlWindPersuitCycle_CheckIfArrived(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_CheckIfArrived orig, WhirlWindPersuitCycle self)
        {
            // Don't stop whirlwind if in danger of falling
            if (!FallRiskMitigator.IsInDangerOfFalling(self.characterBody))
            {
                orig(self);
            }
        }

        private void GenericSkill_Awake(On.RoR2.GenericSkill.orig_Awake orig, GenericSkill self)
        {
            if (self.TryGetComponent<Halcyonite2BodyBehavior>(out var behavior) && self._skillFamily is null)
            {
                behavior.SetupSkill(self);
            }

            orig(self);
        }

        private void BaseAI_UpdateBodyAim(On.RoR2.CharacterAI.BaseAI.orig_UpdateBodyAim orig, BaseAI self, float deltaTime)
        {
            this.MaybeFreezeAim(self);
            orig(self, deltaTime);
        }

        private void MaybeFreezeAim(BaseAI ai)
        {
            if (!ai.body || !ai.body.TryGetComponent<Halcyonite3BodyBehavior>(out var behavior) || behavior.CurrentThrustContext != Halcyonite3BodyBehavior.ThrustContext.PostLaser)
            {
                return;
            }

            EntityState? weaponState = behavior.WeaponStateMachine?.state;

            if (weaponState is GoldenSwipe)
            {
                ai.aimVectorMaxSpeedOverride = 1;
            }
        }

        private void SetStateOnHurt_OnTakeDamageServer(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, SetStateOnHurt self, DamageReport damageReport)
        {
            orig(self, damageReport);

            if (!self.targetStateMachine || !self.spawnedOverNetwork)
            {
                return;
            }

            var damageInfo = damageReport.damageInfo;

            if (damageInfo.procCoefficient >= Mathf.Epsilon && damageInfo.HasModdedDamageType(Content.DamageTypes.Stun1sBypassImmunity))
            {
                EffectManager.SimpleImpactEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ImpactStunGrenade"), damageInfo.position, -damageInfo.force, transmit: true);
                self.SetStunBypassImmunity(1f);
            }
        }

        private void BasicMeleeAttack_OnExit(On.EntityStates.BasicMeleeAttack.orig_OnExit orig, BasicMeleeAttack self)
        {
            if (self is GoldenSwipe && self.characterBody && self.characterBody.TryGetComponent<Halcyonite3BodyBehavior>(out var behavior))
            {
                behavior.Thrusted();
            }

            orig(self);
        }

        private void WhirlWindPersuitCycle_UpdateLand(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_UpdateLand orig, WhirlWindPersuitCycle self)
        {
            orig(self);

            // Exit landing state if in danger of falling. Consider instead ensuring that the
            // landing state isn't entered in the first place if in danger of falling, but this
            // should work for now and is less invasive.
            if (FallRiskMitigator.IsInDangerOfFalling(self.characterBody))
            {
                self.outer.SetNextStateToMain();
            }
        }

        private void GoldenSlash_PlayAnimation(On.EntityStates.Halcyonite.GoldenSlash.orig_PlayAnimation orig, GoldenSlash self)
        {
            orig(self);

            if (self.TryGetComponent<IModifyOverlapAttack>(out var overlapAttackModifier))
            {
                overlapAttackModifier.ModifyOverlapAttack(self);
            }
        }

        private InterruptPriority WhirlWindPersuitCycle_GetMinimumInterruptPriority(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_GetMinimumInterruptPriority orig, WhirlWindPersuitCycle self)
        {
            var original = orig(self);

            if (FallRiskMitigator.IsInDangerOfFalling(self.characterBody) && original < InterruptPriority.PrioritySkill)
            {
                return InterruptPriority.PrioritySkill;
            }

            return original;
        }
    }
}