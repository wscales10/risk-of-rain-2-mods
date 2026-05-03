using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PactOfPunishment.AiSkillDrivers;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.ProtectMonstersFromHazards
{
    public class TryToEscapeFog : Module
    {
        private AISkillDriver.TargetType targetType;

        public interface ICustomSkillDriverEvaluation
        {
            BaseAI.SkillDriverEvaluation? Evaluate(On.RoR2.CharacterAI.BaseAI.orig_EvaluateSingleSkillDriver orig, BaseAI self, ref BaseAI.SkillDriverEvaluation currentSkillDriverEvaluation, AISkillDriver aiSkillDriver, float myHealthFraction);
        }

        public static TryToEscapeFog Instance { get; } = new TryToEscapeFog();

        public override void Init()
        {
            On.RoR2.CharacterAI.BaseAI.EvaluateSingleSkillDriver += this.BaseAI_EvaluateSingleSkillDriver;
            this.targetType = ModdedTargetType.Register(ai => ai.GetComponent<SafeZoneTargetHolder>()?.Target);
            On.RoR2.CharacterAI.BaseAI.Awake += this.BaseAI_Awake;
            IL.RoR2.CharacterAI.BaseAI.Target.GetBullseyePosition += Utils.HookIL(Target_GetBullseyePosition);
            On.RoR2.SceneInfo.Start += this.SceneInfo_Start;
            On.RoR2.CharacterAI.BaseAI.SetGoalPosition_Target += this.BaseAI_SetGoalPosition_Target;

            /*
             * Some more ideas:
             * - Let monsters drop down off ledges more often (e.g. the one in Rallypoint Delta)
             * - Reject path if total distance score is too high? (not good if stuck in fog)
             */
        }

        public void InsertSkillDriver(BaseAI ai, Action<AISkillDriver> setupAction, int index)
        {
            var newSkillDriver = ai.gameObject.AddComponent<CustomSkillDriver>();

            newSkillDriver.moveTargetType = this.targetType;
            newSkillDriver.minDistance = 5;

            setupAction(newSkillDriver);
            ai.InsertSkillDriver(newSkillDriver, index);
        }

        private static void Target_GetBullseyePosition(ILCursor c)
        {
            c.GotoLast(MoveType.AfterLabel,
                x => x.MatchLdcI4(0),
                x => x.MatchRet());
            c.Remove();
            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<BaseAI.Target, bool>>(self => self.gameObject && !self.characterBody);
        }

        private void BaseAI_SetGoalPosition_Target(On.RoR2.CharacterAI.BaseAI.orig_SetGoalPosition_Target orig, BaseAI self, BaseAI.Target goalTarget)
        {
            orig(self, goalTarget);
            if (self.broadNavigationSystem is NodeGraphNavigationSystem nodeGraphNavigationSystem)
            {
                ref var reference = ref nodeGraphNavigationSystem.allAgentData[(int)self.broadNavigationAgent.handle];

                if (reference.pathRequest is TryNotToNavigateThroughTheVoidFog.DangerAwarePathRequest dangerAwarePathRequest)
                {
                    dangerAwarePathRequest.UseTerminationPredicate = goalTarget is SafeZoneTarget;
                }
                else
                {
                    this.Logger.LogError("Expected path request to be a DangerAwarePathRequest");
                }
            }
        }

        private void SceneInfo_Start(On.RoR2.SceneInfo.orig_Start orig, SceneInfo self)
        {
            orig(self);
            AddDropLinksToNodeGraphs.AddDropLinksToNodeGraph(self.groundNodes);
        }

        // TODO: I think this could maybe use some form of ObstacleNavigator?
        private void InsertSkillDriver(BaseAI ai)
        {
            var indexOfLastSkillDriverWithSkillSlot = Array.FindLastIndex(ai.skillDrivers, skillDriver => skillDriver.skillSlot != RoR2.SkillSlot.None);
            this.InsertSkillDriver(ai, newSkillDriver =>
            {
                newSkillDriver.customName = "SeekSafeWard";
                newSkillDriver.skillSlot = SkillSlot.None;
                newSkillDriver.shouldSprint = ai.skillDrivers.Any(x => x.shouldSprint);
                newSkillDriver.ignoreNodeGraph = false;
            }, indexOfLastSkillDriverWithSkillSlot + 1);
        }

        private void BaseAI_Awake(On.RoR2.CharacterAI.BaseAI.orig_Awake orig, BaseAI self)
        {
            orig(self);

            if (Run.instance is InfiniteTowerRun run && run.safeWardController)
            {
                self.gameObject.AddComponent<SafeZoneTargetHolder>().AI = self;
                this.InsertSkillDriver(self);
            }
        }

        private BaseAI.SkillDriverEvaluation? BaseAI_EvaluateSingleSkillDriver(On.RoR2.CharacterAI.BaseAI.orig_EvaluateSingleSkillDriver orig, BaseAI self, ref BaseAI.SkillDriverEvaluation currentSkillDriverEvaluation, AISkillDriver aiSkillDriver, float myHealthFraction)
        {
            if (aiSkillDriver is ICustomSkillDriverEvaluation customSkillDriverEvaluation)
            {
                return customSkillDriverEvaluation.Evaluate(orig, self, ref currentSkillDriverEvaluation, aiSkillDriver, myHealthFraction);
            }

            return orig(self, ref currentSkillDriverEvaluation, aiSkillDriver, myHealthFraction);
        }

        // TODO: this method shouldn't really be on the AISkillDriver.
        public class CustomSkillDriver : AISkillDriver, ICustomSkillDriverEvaluation
        {
            public BaseAI.SkillDriverEvaluation? Evaluate(On.RoR2.CharacterAI.BaseAI.orig_EvaluateSingleSkillDriver orig, BaseAI self, ref BaseAI.SkillDriverEvaluation currentSkillDriverEvaluation, AISkillDriver aiSkillDriver, float myHealthFraction)
            {
                var originalResult = orig(self, ref currentSkillDriverEvaluation, aiSkillDriver, myHealthFraction);

                if (originalResult == null)
                {
                    return null;
                }

                if (!(Run.instance is InfiniteTowerRun run))
                {
                    return null;
                }

                if (run.fogDamageController && run.fogDamageController.safeZones.Any(x => x.IsInBounds(self.bodyTransform.position)))
                {
                    return null;
                }

                return originalResult;
            }
        }

        [Serializable]
        public class SafeZoneTarget : BaseAI.Target
        {
            public SafeZoneTarget([NotNull] BaseAI owner) : base(owner)
            {
            }
        }

        public class SafeZoneTargetHolder : MonoBehaviour
        {
            public SafeZoneTarget? Target { get; private set; }

            public BaseAI? AI
            {
                get => this.Target?.owner;

                internal set
                {
                    if (this.Target?.owner == value)
                    {
                        return;
                    }

                    if (value == null)
                    {
                        this.Target = null;
                        return;
                    }

                    this.Target = new SafeZoneTarget(value);
                    if (Run.instance is InfiniteTowerRun run && run.safeWardController)
                    {
                        this.Target.gameObject = run.safeWardController.gameObject;
                    }
                }
            }
        }
    }
}