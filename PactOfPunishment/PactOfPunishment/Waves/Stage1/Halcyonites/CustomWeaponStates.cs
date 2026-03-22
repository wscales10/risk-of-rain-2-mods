using BepInEx.Logging;
using EntityStates;
using RoR2.Skills;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using R2API;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    public static partial class CustomWeaponStates
    {
        internal static void Init(ManualLogSource logger)
        {
            CrossedFistsSkillState.customSkill = new CrossedFistsSkillBuilder().AddSkill(logger);
            RepeatingFistSkillState.customSkill = new RepeatingFistSkillBuilder().AddSkill(logger);
            ContentAddition.AddSkillFamily(RepeatingFistSkillState.skillFamily = MakeRepeatingFistsSkillFamily());
            LineOfFistsSkillState.customSkill = new LineOfFistsSkillBuilder().AddSkill(logger);

            ImmobileState.shieldRemovalEffectPrefab = Utils.BeginLoad<GameObject>("RoR2/Base/goldshores/GoldshoresArmorRemoval.prefab", logger);
            Utils.AddEntityState<ImmobileState>(logger);
        }

        private static SkillFamily MakeRepeatingFistsSkillFamily()
        {
            var skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            ((UnityEngine.Object)skillFamily).name = "RepeatingFistSkillFamily";
            skillFamily.variants = new[] { new SkillFamily.Variant { skillDef = RepeatingFistSkillState.customSkill.SkillDef } };
            skillFamily.catalogIndex = -1;
            return skillFamily;
        }

        public abstract class FistsSkillState : BaseState
        {
            public Durations durations;

            private int totalTimesToFire = 1;

            private CharacterBody? targetBody;

            private float damageCoefficient = 1f;

            private BaseAI? ai;

            private float duration;

            /// <summary>
            /// Relative to state entry time.
            /// </summary>
            private float createZonesTimeStamp;

            private ulong seed;

            private int timesFired;

            protected Xoroshiro128Plus rng { get; private set; }

            protected abstract Durations BaseDurations { get; }

            protected abstract GameObject ZoneProjectilePrefab { get; }

            public override void OnEnter()
            {
                base.OnEnter();
                this.totalTimesToFire = this.GetTotalTimesToFire();
                this.durations = this.BaseDurations.ScaledWithAttackSpeed(this.attackSpeedStat);
                this.duration = this.durations.GetTotal(this.totalTimesToFire);
                this.createZonesTimeStamp = this.durations.windUp;
                this.ai = this.characterBody.master.GetComponent<BaseAI>();
                if (this.isAuthority)
                {
                    this.seed = (ulong)DateTime.UtcNow.Ticks;
                }
                this.rng = new Xoroshiro128Plus(this.seed);
            }

            public override InterruptPriority GetMinimumInterruptPriority()
            {
                return InterruptPriority.PrioritySkill;
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (this.timesFired < this.totalTimesToFire && this.fixedAge > this.createZonesTimeStamp)
                {
                    if (this.isAuthority)
                    {
                        this.outer.StartCoroutine(this.CreateZones(this.timesFired));
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

            protected static bool MoveTargetToGround(Vector3 target, out Vector3 result)
            {
                if (Physics.Raycast(target, Vector3.down, out var hitInfo, 1000f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    result = hitInfo.point;
                    return true;
                }

                result = target;
                return false;
            }

            protected abstract int GetTotalTimesToFire();

            protected abstract IEnumerable<Vector3?> GetZoneCentres(ZoneCreationArgs args);

            private static bool IsValidTarget(CharacterBody? target)
            {
                return target && target.master && !target.master.lostBodyToDeath;
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

            private IEnumerator CreateZones(int zoneBatchIndex)
            {
                int zonesToCreate = this.rng.RangeInt(3, 5);
                List<int> indices = Enumerable.Range(0, 5).ToList();

                while (indices.Count > zonesToCreate)
                {
                    indices.RemoveAt(this.rng.RangeInt(0, indices.Count));
                }

                this.CheckTarget();
                CharacterBody target = this.targetBody ?? this.characterBody;
                Vector3 targetPosition = target.transform.position;

                ZoneCreationArgs args = new ZoneCreationArgs
                {
                    ZoneBatchIndex = zoneBatchIndex,
                    TargetPosition = targetPosition,
                    TargetHorizontalFacingDirection = target.GetHorizontalFacingDirection(),
                    MyHorizontalFacingDirection = this.characterBody.GetHorizontalFacingDirection(),
                    MyCurrentPosition = this.characterBody.corePosition,
                };

                if (!MoveTargetToGround(targetPosition, out var groundPosition))
                {
                    var nodeIndex = SceneInfo.instance.groundNodes.FindClosestNode(targetPosition, HullClassification.Human);
                    SceneInfo.instance.groundNodes.GetNodePosition(nodeIndex, out groundPosition);
                }

                args.TargetGroundPosition = groundPosition;

                bool crit = this.characterBody.RollCrit();

                foreach (var zoneCentre in this.GetZoneCentres(args))
                {
                    if (zoneCentre.HasValue)
                    {
                        this.CreateZone(zoneCentre.Value, crit);
                    }

                    yield return new WaitForSeconds(this.durations.createZoneCooldown);
                }
            }

            private void CreateZone(Vector3 centre, bool crit)
            {
                if (!this.isAuthority)
                {
                    return;
                }

                var fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = this.ZoneProjectilePrefab,
                    position = centre,
                    rotation = Quaternion.identity, // TODO: use c?
                    owner = this.gameObject,
                    damage = this.damageStat * this.damageCoefficient,
                    crit = crit,
                    fuseOverride = this.durations.zoneFuse
                };

                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
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
                        zoneFuse = this.zoneFuse,
                    };
                }
            }

            protected struct ZoneCreationArgs
            {
                public int ZoneBatchIndex { get; set; }

                public Vector3 TargetHorizontalFacingDirection { get; set; }

                public Vector3 TargetGroundPosition { get; set; }

                public Vector3 TargetPosition { get; set; }

                public Vector3 MyHorizontalFacingDirection { get; set; }

                public Vector3 MyCurrentPosition { get; set; }
            }
        }

        // TODO: test extensively in multiplayer
        public class CrossedFistsSkillState : FistsSkillState
        {
            public static readonly Vector2[] normalizedCentrePoints = new Vector2[]
            {
                Vector2.zero,
                Mathf.Sqrt(2) * Vector2.up,
                Mathf.Sqrt(2) * Vector2.right,
                Mathf.Sqrt(2) * Vector2.down,
                Mathf.Sqrt(2) * Vector2.left,
            };

            public static GameObject zoneProjectilePrefab;

            internal static CustomBuiltSkill customSkill;

            private static float zoneRadius = 7f;

            protected override Durations BaseDurations => new Durations
            {
                endLag = 1 / 3f,
                windUp = 1 / 60f,
                createZonesCooldown = 55 / 60f,
                createZoneCooldown = 1 / 60f,
                zoneFuse = 1.2f,
            };

            protected override GameObject ZoneProjectilePrefab => zoneProjectilePrefab;

            protected override IEnumerable<Vector3?> GetZoneCentres(ZoneCreationArgs args)
            {
                int zonesToCreate = this.rng.RangeInt(3, 5);
                List<int> indices = Enumerable.Range(0, 5).ToList();

                while (indices.Count > zonesToCreate)
                {
                    indices.RemoveAt(this.rng.RangeInt(0, indices.Count));
                }

                return indices.Select(i => GetZoneCentre(normalizedCentrePoints[i], args.TargetHorizontalFacingDirection, args.TargetGroundPosition));
            }

            protected override int GetTotalTimesToFire() => 4;

            private static Vector3? GetZoneCentre(Vector2 d, Vector3 c, Vector3 o)
            {
                var diff = new Vector2(d.x * c.z + d.y * c.x, d.y * c.z - d.x * c.x) * zoneRadius;
                return o + new Vector3(diff.x, 0, diff.y);
            }
        }

        public class LineOfFistsSkillState : FistsSkillState
        {
            internal static CustomBuiltSkill customSkill;

            private bool? isTargetToTheRight;

            internal static float ZoneRadius { get; private set; } = 7f;

            internal static int ZonesToCreate { get; private set; } = 5;

            protected override Durations BaseDurations => new Durations
            {
                createZoneCooldown = 0.075f,
                createZonesCooldown = 1 / 30f,
                zoneFuse = 1.15f,
                windUp = 1 / 30f,
                endLag = 2.3f,
            };

            protected override GameObject ZoneProjectilePrefab => CrossedFistsSkillState.zoneProjectilePrefab; // TODO: seems wrong to do this

            protected override int GetTotalTimesToFire() => (this.characterBody.TryGetComponent<Halcyonite3BodyBehavior>(out var behavior) && behavior.isThreeWayFistsEnabled) ? 3 : 1;

            protected override IEnumerable<Vector3?> GetZoneCentres(ZoneCreationArgs args) // TODO: at less than 33% health, also fire lines to left and right
            {
                this.isTargetToTheRight ??= Vector3.Cross(args.MyHorizontalFacingDirection, args.TargetPosition - args.MyCurrentPosition).y > 0;

                Vector3 lineDirection;

                switch (args.ZoneBatchIndex)
                {
                    case 0:
                        lineDirection = args.MyHorizontalFacingDirection;
                        break;

                    case 1:
                        if (this.isTargetToTheRight.Value)
                        {
                            lineDirection = new Vector3(args.MyHorizontalFacingDirection.z, 0, -args.MyHorizontalFacingDirection.x);
                        }
                        else
                        {
                            lineDirection = new Vector3(-args.MyHorizontalFacingDirection.z, 0, args.MyHorizontalFacingDirection.x);
                        }
                        break;

                    case 2:
                        if (this.isTargetToTheRight.Value)
                        {
                            lineDirection = new Vector3(-args.MyHorizontalFacingDirection.z, 0, args.MyHorizontalFacingDirection.x);
                        }
                        else
                        {
                            lineDirection = new Vector3(args.MyHorizontalFacingDirection.z, 0, -args.MyHorizontalFacingDirection.x);
                        }
                        break;

                    default:
                        yield break;
                }

                for (int i = 0; i < ZonesToCreate; i++)
                {
                    Vector3 targetPosition = args.MyCurrentPosition + lineDirection * ZoneRadius * (i + 1);
                    float rayLength = 60f;
                    if (Physics.Raycast(new Ray(targetPosition + Vector3.up * (rayLength / 2f), Vector3.down), out var hitInfo, rayLength, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                    {
                        targetPosition = hitInfo.point;
                    }

                    yield return targetPosition;
                }
            }
        }

        public class RepeatingFistSkillState : FistsSkillState
        {
            internal static CustomBuiltSkill customSkill;

            internal static SkillFamily skillFamily;

            protected override Durations BaseDurations => new Durations
            {
                createZonesCooldown = this.IsPoweredUp ? 0.8f : 1.2f,
                zoneFuse = 0.95f,
                windUp = 1 / 60f,
                endLag = this.IsPoweredUp ? 1.6f : 2,
            };

            protected override GameObject ZoneProjectilePrefab => CrossedFistsSkillState.zoneProjectilePrefab;

            private bool IsPoweredUp => this.characterBody.TryGetComponent<Halcyonite2BodyBehavior>(out var behavior) && behavior.powerMeter.IsPoweredUp;

            protected override int GetTotalTimesToFire() => this.IsPoweredUp ? 8 : 6;

            protected override IEnumerable<Vector3?> GetZoneCentres(ZoneCreationArgs args)
            {
                yield return args.TargetGroundPosition;
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