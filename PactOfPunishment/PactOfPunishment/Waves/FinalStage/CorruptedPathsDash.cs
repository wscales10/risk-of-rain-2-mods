using EntityStates;
using EntityStates.FalseSonBoss;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PactOfPunishment.Waves.Common;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Waves.FinalStage
{
    public partial class FinalBoss : Module
    {
        private static void CorruptedPathsDash_FixedUpdate(ILCursor c)
        {
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchCall<EntityState>($"get_{nameof(EntityState.inputBank)}"),
                x => x.MatchCallvirt<InputBankTest>($"get_{nameof(InputBankTest.aimDirection)}"),
                x => x.MatchStfld<CorruptedPathsDash>(nameof(CorruptedPathsDash.dashVector)));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<CorruptedPathsDash>>((self) =>
            {
                if (self.TryGetComponent<PredictiveDashController>(out var predictiveDashController) && predictiveDashController.TryOverrideDashTargetPosition(self.fixedAge, out var targetPosition))
                {
                    self.inputBank.aimDirection = targetPosition - self.transform.position;
                    CorruptedPathsDash.dashDuration = self.GetComponent<CorruptedPathsDashInfo>().CalculateDashDuration(targetPosition, self.fixedAge);
                }
            });
            while (c.TryGotoNext(x => x.MatchLdsfld<CorruptedPathsDash>(nameof(CorruptedPathsDash.dashDuration))))
            {
                c.Remove();
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<CorruptedPathsDash, float>>(self => self.GetComponent<CorruptedPathsDashInfo>().dashDuration);
            }
        }

        private static void CorruptedPathsDash_OnEnter(ILCursor c)
        {
            c.GotoNext(x => x.MatchLdsfld<CorruptedPathsDash>(nameof(CorruptedPathsDash.gapToEnemy)));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<CorruptedPathsDash, float>>(self => self.gameObject.EnsureComponent<CorruptedPathsDashInfo>().GapToEnemy);

            c.GotoNext(x => x.MatchStsfld<CorruptedPathsDash>(nameof(CorruptedPathsDash.dashDuration)));
            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<float, CorruptedPathsDash>>((orig, self) => self.gameObject.EnsureComponent<CorruptedPathsDashInfo>().dashDuration = orig);
        }

        private void InitCorruptedPathsDash()
        {
            // Base
            On.EntityStates.FalseSonBoss.CorruptedPathsDash.GetNextStateAuthority += this.CorruptedPathsDash_GetNextStateAuthority;
            IL.EntityStates.FalseSonBoss.CorruptedPathsDash.OnEnter += Utils.HookIL(CorruptedPathsDash_OnEnter);
            IL.EntityStates.FalseSonBoss.CorruptedPathsDash.FixedUpdate += Utils.HookIL(CorruptedPathsDash_FixedUpdate);
            On.EntityStates.FalseSonBoss.CorruptedPathsDash.OnEnter += this.CorruptedPathsDash_OnEnter;
        }

        private void CorruptedPathsDash_OnEnter(On.EntityStates.FalseSonBoss.CorruptedPathsDash.orig_OnEnter orig, CorruptedPathsDash self)
        {
            this.Logger.LogDebug($"False Son Boss Dashing at {Run.instance.GetRunStopwatch()}");
            self.gameObject.EnsureComponent<CorruptedPathsDashInfo>().OnDash();

            if (self.TryGetComponent<UpgradedLeap.UpgradedLeapGhostBehavior>(out var behavior))
            {
                behavior.OnDashEnter();
            }

            if (self.TryGetComponent<PredictiveDashController>(out var predictiveDashController))
            {
                predictiveDashController.OnDashEnter();
            }

            orig(self);
        }

        private EntityState CorruptedPathsDash_GetNextStateAuthority(On.EntityStates.FalseSonBoss.CorruptedPathsDash.orig_GetNextStateAuthority orig, CorruptedPathsDash self)
        {
            if (self.GetComponent<UpgradedLeap.UpgradedLeapGhostBehavior>())
            {
                return Utils.InstantiateState<FissureSlamWindup>();
            }

            self.skillLocator.primary.UnsetSkillOverride(self.characterBody, this.swatAwayPlayersSkillDef, GenericSkill.SkillOverridePriority.Contextual);

            var fissureSlamSkill = self.skillLocator.FindSkill("FissureSlam");

            if (fissureSlamSkill.stock < fissureSlamSkill.skillDef.requiredStock)
            {
                fissureSlamSkill.RestockSteplike();
            }

            return EntityStateCatalog.InstantiateState(ref self.outer.mainStateType);
        }

        public class PredictiveDashController : MonoBehaviour
        {
#if DEBUG
            static PredictiveDashController()
            {
                IsDebugDisplayEnabled = true;
            }
#endif

            public Func<float> GetLookAheadDuration;

            public Func<bool>? ShouldUsePredictedPosition;

            public Func<Vector3, Vector3>? GetDashTargetPositionFromPredictedPosition;

            public CharacterBody? TargetBodyOverride;

            private static bool isDebugDisplayEnabled = false;

            private TargetingAndPredictionController targetingAndPredictionController;

            private GameObject dashTargetSphere;

            private GameObject predictedPositionSphere;

            private static event Action? DebugDisplayToggled;

            public static bool IsDebugDisplayEnabled
            {
                get => isDebugDisplayEnabled;

                set
                {
                    isDebugDisplayEnabled = value;
                    DebugDisplayToggled?.Invoke();
                }
            }

            public bool IsPredictorActive { get; private set; }

            public bool DidMostRecentDashUsePredictedPosition { get; private set; }

            public void Awake()
            {
                DebugDisplayToggled += this.PredictiveDashController_DebugDisplayToggled;
                this.dashTargetSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                this.dashTargetSphere.GetComponent<Renderer>().material.color = Color.green;
                this.predictedPositionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                this.predictedPositionSphere.GetComponent<Renderer>().material.color = Color.red;
                this.PredictiveDashController_DebugDisplayToggled();
                this.targetingAndPredictionController = this.gameObject.AddComponent<TargetingAndPredictionController>();
            }

            public void OnDestroy()
            {
                DebugDisplayToggled -= this.PredictiveDashController_DebugDisplayToggled;
                Destroy(this.dashTargetSphere);
                Destroy(this.predictedPositionSphere);
            }

            internal void OnDashEnter()
            {
                if (this.ShouldUsePredictedPosition?.Invoke() ?? true)
                {
                    this.DidMostRecentDashUsePredictedPosition = true;
                    this.IsPredictorActive = true;
                    this.targetingAndPredictionController.StartPredictTarget(this.TargetBodyOverride?.transform, () => this.IsPredictorActive = false);
                }
                else
                {
                    this.DidMostRecentDashUsePredictedPosition = false;
                    this.IsPredictorActive = false;
                }
            }

            internal bool TryOverrideDashTargetPosition(float dashStateFixedAge, out Vector3 dashTargetPosition)
            {
                this.IsPredictorActive = false;

                if (this.DidMostRecentDashUsePredictedPosition && this.targetingAndPredictionController.GetPredictionPositionConsumePredictor(Mathf.Max(1 / 60f, this.GetLookAheadDuration() - dashStateFixedAge), out var predictedPosition))
                {
                    this.predictedPositionSphere.SetActive(true);
                    this.predictedPositionSphere.transform.position = predictedPosition;

                    if (this.GetDashTargetPositionFromPredictedPosition is null)
                    {
                        dashTargetPosition = predictedPosition;
                    }
                    else
                    {
                        dashTargetPosition = this.GetDashTargetPositionFromPredictedPosition(predictedPosition);
                    }

                    this.dashTargetSphere.SetActive(true);
                    this.dashTargetSphere.transform.position = dashTargetPosition;

                    return true;
                }
                else
                {
                    this.predictedPositionSphere.SetActive(false);
                    this.dashTargetSphere.SetActive(false);
                    dashTargetPosition = default;
                    return false;
                }
            }

            private void PredictiveDashController_DebugDisplayToggled()
            {
                this.dashTargetSphere?.SetActive(IsDebugDisplayEnabled);
                this.predictedPositionSphere?.SetActive(IsDebugDisplayEnabled);
            }
        }

        public class CorruptedPathsDashInfo : BossBodyBehavior
        {
            public float dashDuration;

            private float dashTimer;

            public float GapToEnemy
            {
                get
                {
                    if (this.GetComponent<FinalBossUpgradeStrategies.BodyBehavior>() && this.dashTimer > 0)
                    {
                        return 0;
                    }

                    return CorruptedPathsDash.gapToEnemy;
                }
            }

            public float CalculateDashDuration(Vector3 targetPosition, float fixedAge)
            {
                float output = Vector3.Distance(targetPosition, this.transform.position) * 0.1f / CorruptedPathsDash.speedCoefficient - this.GapToEnemy;
                output = Mathf.Max(output, fixedAge - CorruptedPathsDash.dashPrepDuration);
                this.dashDuration = output;
                return output;
            }

            internal void OnDash()
            {
                this.dashTimer = 3;
            }

            protected override void ManagedFixedUpdate(float deltaTime)
            {
                base.ManagedFixedUpdate(deltaTime);
                this.dashTimer = Mathf.Max(0, this.dashTimer - deltaTime * this.Body.attackSpeed);
            }
        }
    }
}