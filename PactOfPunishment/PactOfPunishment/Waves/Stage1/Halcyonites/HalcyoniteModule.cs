using EntityStates;
using EntityStates.Halcyonite;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    [RequireComponent(typeof(ChildMonsterController))]
    public class DisableChildMonsterTeleport : MonoBehaviour
    {
    }

    public class HalcyoniteModule : Module
    {
        public static SkillDef WhirlwindSkillDef;

        public override void Init()
        {
            On.EntityStates.Halcyonite.GoldenSwipe.PlayAnimation += GoldenSwipe_PlayAnimation;
            IL.EntityStates.Halcyonite.ChargeTriLaser.OnEnter += Utils.HookIL(EnsureMinAttackSpeedForHalcyoniteIL);
            IL.EntityStates.Halcyonite.TriLaser.OnEnter += Utils.HookIL(EnsureMinAttackSpeedForHalcyoniteIL);
            IL.EntityStates.Halcyonite.TriLaser.FireTriLaser += Utils.HookIL(EnsureMinAttackSpeedForHalcyoniteIL);

            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateDash += Utils.HookIL(WhirlWindPersuitCycle_UpdateDash);
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.OnEnter += Utils.HookIL(LogOnSetWhirlWindPersuitStateIL);
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateDecelerate += Utils.HookIL(LogOnSetWhirlWindPersuitStateIL);
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateDash += Utils.HookIL(LogOnSetWhirlWindPersuitStateIL);
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.CheckIfArrived += Utils.HookIL(LogOnSetWhirlWindPersuitStateIL);
            IL.EntityStates.Halcyonite.WhirlWindPersuitCycle.UpdateFindTarget += Utils.HookIL(LogOnSetWhirlWindPersuitStateIL);
            On.EntityStates.Halcyonite.WhirlWindPersuitCycle.OnExit += WhirlWindPersuitCycle_OnExit;

            this.AddSkill();
            ImmobileState.shieldRemovalEffectPrefab = Utils.BeginLoad<GameObject>("RoR2/Base/goldshores/GoldshoresArmorRemoval.prefab", this.Logger);
            Utils.AddEntityState<ImmobileState>(this.Logger);
            On.EntityStates.Halcyonite.WhirlWindPersuitCycle.CheckIfArrived += WhirlWindPersuitCycle_CheckIfArrived;
            On.RoR2.ChildMonsterController.CheckTeleportAvailable += ChildMonsterController_CheckTeleportAvailable;
        }

        private static void WhirlWindPersuitCycle_UpdateDash(ILCursor c)
        {
            ILLabel? label = null;
            c.GotoNext(x => x.MatchLdarg(0),
                x => x.MatchLdfld<WhirlWindPersuitCycle>(nameof(WhirlWindPersuitCycle.dashSafeOutTime)),
                x => x.MatchLdsfld<WhirlWindPersuitCycle>(nameof(WhirlWindPersuitCycle.dashSafeExitDuration)),
                x => x.MatchBleUn(out label));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<WhirlWindPersuitCycle, bool>>(self => IsInDangerOfFalling(self.characterBody));
            c.Emit(OpCodes.Brtrue_S, label);
        }

        internal static bool IsInDangerOfFalling(CharacterBody body)
        {
            bool? isAboveGround;

            if (!body)
            {
                isAboveGround = null;
            }
            else if (body.TryGetComponent<HalcyoniteBodyBehavior>(out var behavior))
            {
                isAboveGround = behavior.IsAboveGround;
            }
            else
            {
                isAboveGround = IsAboveGroundInternal(body.transform);
            }

            return isAboveGround == false;
        }

        internal static bool IsAboveGroundInternal(Transform transform)
        {
            return Physics.Raycast(transform.position, Vector3.down, 35, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
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

        private static void EnsureMinAttackSpeedForHalcyoniteIL(ILCursor c)
        {
            while (c.TryGotoNext(MoveType.AfterLabel, x => x.MatchLdfld<BaseState>(nameof(BaseState.attackSpeedStat))))
            {
                c.Remove();
                c.EmitDelegate<Func<BaseState, float>>(self =>
                {
                    var value = self.attackSpeedStat;

                    if (self.GetComponent<Halcyonite1.Halcyonite1BodyBehavior>())
                    {
                        value = Mathf.Max(1, value);
                    }

                    return value;
                });
            }
        }

        private static void GoldenSwipe_PlayAnimation(On.EntityStates.Halcyonite.GoldenSwipe.orig_PlayAnimation orig, GoldenSwipe self)
        {
            orig(self);

            if (self.TryGetComponent<Halcyonite1.Halcyonite1BodyBehavior>(out var behavior))
            {
                behavior.OnSwipe(self);
            }
        }

        private static void WhirlWindPersuitCycle_CheckIfArrived(On.EntityStates.Halcyonite.WhirlWindPersuitCycle.orig_CheckIfArrived orig, WhirlWindPersuitCycle self)
        {
            // Don't stop whirlwind if in danger of falling
            if (!IsInDangerOfFalling(self.characterBody))
            {
                orig(self);
            }
        }

        private void AddSkill()
        {
            Utils.OnLoad<SkillDef>("RoR2/DLC2/Halcyonite/HalcyoniteMonsterWhirlwindRush.asset", x => WhirlwindSkillDef = x);

            var skillDef = ScriptableObject.CreateInstance<SkillDef>();

            Utils.OnLoad<GameObject>("RoR2/Base/Titan/TitanGoldPreFistProjectile.prefab", x => UntitledSkillState.zoneProjectilePrefab = x);

            skillDef.activationState = Utils.AddEntityState<UntitledSkillState>(this.Logger);
            skillDef.activationStateMachineName = "Weapon";
            skillDef.baseMaxStock = 1;
            skillDef.baseRechargeInterval = 12f;
            skillDef.beginSkillCooldownOnSkillEnd = false;
            skillDef.canceledFromSprinting = false;
            skillDef.cancelSprintingOnActivation = true;
            skillDef.fullRestockOnAssign = true;
            skillDef.interruptPriority = InterruptPriority.Skill; // TODO: could be PrioritySkill, but try this for now. MAKE SURE THAT IT IS LESS THAN THE STATE'S MINIMUM INTERRUPT PRIORITY!
            skillDef.isCombatSkill = true;
            skillDef.mustKeyPress = false;
            skillDef.rechargeStock = 1;
            skillDef.requiredStock = 1;
            skillDef.stockToConsume = 1;

            ContentAddition.AddSkillDef(skillDef);
            UntitledSkillState.skillDef = skillDef;
        }

        // TODO: test extensively in multiplayer
        public class UntitledSkillState : BaseState
        {
            public static readonly Vector2[] normalizedCentrePoints = new Vector2[]
            {
                Vector2.zero,
                Mathf.Sqrt(2) * Vector2.up,
                Mathf.Sqrt(2) * Vector2.right,
                Mathf.Sqrt(2) * Vector2.down,
                Mathf.Sqrt(2) * Vector2.left,
            };

            public static readonly Durations baseDurations = new Durations
            {
                // TODO: I've halved all of these as I want my halcyonite to have half attack speed.
                // I need to consider whether all of these should be scaled with attack speed, and
                // if not adjust them and the scaling method accordingly.
                endLag = 1 / 3f,
                windUp = 1 / 60f,
                createZonesCooldown = 55 / 60f,
                createZoneCooldown = 1 / 60f,
                zoneFuse = 18 / 30f,
            };

            public static GameObject zoneProjectilePrefab;

            public static SkillDef skillDef;

            public static float zoneRadius = 7f;

            public Durations durations;

            public int totalTimesToFire = 4;

            private int timesFired;

            /// <summary>
            /// Relative to state entry time.
            /// </summary>
            private float createZonesTimeStamp;

            private float duration;

            private Xoroshiro128Plus rng;

            private float damageCoefficient = 1f;

            private BaseAI? ai;

            private CharacterBody? targetBody;

            private ulong seed;

            public static void SetupSkillDriver(AISkillDriver newSkillDriver)
            {
                newSkillDriver.customName = "New Skill";
                newSkillDriver.skillSlot = SkillSlot.Utility;
                newSkillDriver.requiredSkill = skillDef;
                newSkillDriver.requireSkillReady = true;
                newSkillDriver.requireEquipmentReady = false;
                newSkillDriver.minDistance = 0;
                newSkillDriver.maxDistance = 200;
                newSkillDriver.selectionRequiresTargetLoS = false;
                newSkillDriver.selectionRequiresOnGround = false;
                newSkillDriver.selectionRequiresTargetNonFlier = false;
                newSkillDriver.selectionRequiresAimTarget = false;
                newSkillDriver.maxTimesSelected = -1;
                newSkillDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                newSkillDriver.activationRequiresTargetLoS = false;
                newSkillDriver.activationRequiresAimTargetLoS = false;
                newSkillDriver.activationRequiresAimConfirmation = false;
                newSkillDriver.movementType = AISkillDriver.MovementType.Stop;
                newSkillDriver.moveInputScale = 0;
                newSkillDriver.aimType = AISkillDriver.AimType.None; // TODO: AtCurrentEnemy?
                newSkillDriver.ignoreNodeGraph = false;
                newSkillDriver.shouldSprint = false;
                newSkillDriver.shouldFireEquipment = false;
                newSkillDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                newSkillDriver.resetCurrentEnemyOnNextDriverSelection = false;
                newSkillDriver.noRepeat = false;

                // newSkillDriver.nextHighPriorityOverride = ; newSkillDriver.enabled = true;
                newSkillDriver.useGUILayout = true;
            }

            public override void OnEnter()
            {
                base.OnEnter();
                this.durations = baseDurations.ScaledWithAttackSpeed(this.attackSpeedStat);
                this.duration = this.durations.GetTotal(this.totalTimesToFire);
                this.createZonesTimeStamp = this.durations.windUp;
                this.ai = this.characterBody.master.GetComponent<BaseAI>();
                if (this.isAuthority)
                {
                    this.seed = (ulong)DateTime.UtcNow.Ticks;
                }
                this.rng = new Xoroshiro128Plus(this.seed);
            }

            public override void OnExit()
            {
                // Other stuff
                base.OnExit();
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (this.timesFired < this.totalTimesToFire && this.fixedAge > this.createZonesTimeStamp)
                {
                    if (this.isAuthority)
                    {
                        this.outer.StartCoroutine(this.CreateZones());
                    }

                    this.timesFired++;
                    this.createZonesTimeStamp += this.durations.createZonesCooldown;
                }

                if (this.fixedAge >= this.duration && this.timesFired >= this.totalTimesToFire && this.isAuthority)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }

            public override InterruptPriority GetMinimumInterruptPriority()
            {
                return InterruptPriority.PrioritySkill;
            }

            private static bool IsValidTarget(CharacterBody? target)
            {
                return target && target.master && !target.master.lostBodyToDeath;
            }

            private IEnumerator CreateZones()
            {
                int zonesToCreate = this.rng.RangeInt(3, 5);
                List<int> indices = Enumerable.Range(0, 5).ToList();

                while (indices.Count > zonesToCreate)
                {
                    indices.RemoveAt(this.rng.RangeInt(0, indices.Count));
                }

                this.CheckTarget();
                CharacterBody target = this.targetBody ?? this.characterBody;
                var aimDirection = target.inputBank.aimDirection;
                Vector2 targetFacingDirection = new Vector2(aimDirection.x, aimDirection.z).normalized;
                Vector3 targetPosition = target.transform.position;

                if (!this.MoveTargetToGround(targetPosition, out var groundPosition))
                {
                    var nodeIndex = SceneInfo.instance.groundNodes.FindClosestNode(targetPosition, HullClassification.Human);
                    SceneInfo.instance.groundNodes.GetNodePosition(nodeIndex, out groundPosition);
                }

                bool crit = this.characterBody.RollCrit();

                for (int i = 0; i < indices.Count; i++)
                {
                    this.CreateZone(normalizedCentrePoints[indices[i]], targetFacingDirection, groundPosition, crit);
                    yield return new WaitForSeconds(this.durations.createZoneCooldown);
                }
            }

            private void FindNewTarget()
            {
                if (this.ai)
                {
                    var candidate = this.ai.customTarget.characterBody;

                    if (IsValidTarget(candidate))
                    {
                        this.targetBody = candidate;
                        return;
                    }
                }

                var bullseyeSearch = new BullseyeSearch
                {
                    filterByLoS = false,
                    filterByDistinctEntity = true,
                    searchOrigin = this.transform.position,
                    sortMode = BullseyeSearch.SortMode.None,
                    viewer = this.characterBody,
                    teamMaskFilter = this.teamComponent ? TeamMask.GetEnemyTeams(this.GetTeam()) : TeamMask.allButNeutral
                };

                bullseyeSearch.RefreshCandidates();
                var candidates = bullseyeSearch.GetResults().Where(x => x).Select(x => x.healthComponent.body).Where(IsValidTarget).ToArray(); // TODO: consider removing IsValidTarget check

                if (candidates.Length > 0)
                {
                    this.targetBody = this.rng.NextElementUniform(candidates);
                }
            }

            private void CheckTarget()
            {
                if (IsValidTarget(this.targetBody))
                {
                    return;
                }

                this.targetBody = null;
                this.FindNewTarget();
            }

            private void CreateZone(Vector2 d, Vector2 c, Vector3 o, bool crit)
            {
                if (!isAuthority)
                {
                    return;
                }

                var diff = new Vector2(d.x * c.y + d.y * c.x, d.y * c.y - d.x * c.x) * zoneRadius;
                var centre = o + new Vector3(diff.x, 0, diff.y);

                var fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = zoneProjectilePrefab,
                    position = centre,
                    rotation = Quaternion.identity, // TODO: use c?
                    owner = gameObject,
                    damage = this.damageStat * this.damageCoefficient,
                    crit = crit,
                    fuseOverride = this.durations.zoneFuse
                };

                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }

            private bool MoveTargetToGround(Vector3 target, out Vector3 result)
            {
                if (Physics.Raycast(target, Vector3.down, out var hitInfo, 1000f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    result = hitInfo.point;
                    return true;
                }

                result = target;
                return false;
            }

            public struct Durations
            {
                public float endLag;

                public float windUp;

                public float createZonesCooldown; // TODO: maybe should be random +- 0.05 or something? probably not

                public float createZoneCooldown;

                public float zoneFuse;

                public readonly float GetTotal(int totalTimesToFire)
                {
                    return this.windUp + totalTimesToFire * this.createZonesCooldown + this.endLag;
                }

                public Durations ScaledWithAttackSpeed(float attackSpeedStat)
                {
                    return new Durations
                    {
                        createZoneCooldown = this.createZoneCooldown / attackSpeedStat,
                        createZonesCooldown = this.createZonesCooldown / attackSpeedStat,
                        endLag = this.endLag / attackSpeedStat,
                        windUp = this.windUp / attackSpeedStat,
                        zoneFuse = this.zoneFuse / attackSpeedStat,
                    };
                }
            }
        }

        public class ImmobileState : BaseState
        {
            internal static AssetPromise<GameObject> shieldRemovalEffectPrefab;

            public override void OnEnter()
            {
                base.OnEnter();
                this.characterBody.AddBuff(RoR2Content.Buffs.Immune);
                CleanseSystem.CleanseBodyServer(this.characterBody, true, false, false, true, false, false);
            }

            public override void OnExit()
            {
                shieldRemovalEffectPrefab.TryUse(effectPrefab => EffectManager.SpawnEffect(effectPrefab, new EffectData
                {
                    origin = this.characterBody.coreTransform.position
                }, transmit: true));
                this.characterBody.RemoveBuff(RoR2Content.Buffs.Immune);
                base.OnExit();
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();

                if (this.characterMotor)
                {
                    this.characterMotor.velocity = Vector3.zero;
                }
            }

            public override InterruptPriority GetMinimumInterruptPriority()
            {
                return InterruptPriority.Death;
            }
        }
    }
}