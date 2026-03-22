using EntityStates.BrotherMonster;
using EntityStates.Mage;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{
    public class MithrixModule : Module
    {
        public override void Init()
        {
            IL.EntityStates.BrotherMonster.HoldSkyLeap.OnEnter += Utils.HookIL(HoldSkyLeap_OnEnter); // Leap to wave controller spawn target
            IL.EntityStates.BrotherMonster.SprintBash.FixedUpdate += Utils.HookIL(SprintBash_FixedUpdate);
            IL.EntityStates.Mage.FlyUpState.OnEnter += Utils.HookIL(FlyUpState_OnEnter);
            On.EntityStates.AI.BaseAIState.ModifyInputsForJumpIfNeccessary += this.BaseAIState_ModifyInputsForJumpIfNeccessary;
            GenericSkillHooks.IsSkillReady += this.GenericSkillHooks_IsSkillReady;
        }

        private static void HoldSkyLeap_OnEnter(ILCursor c)
        {
            c.GotoNext(MoveType.AfterLabel, x => x.MatchCall<SceneInfo>($"get_{nameof(SceneInfo.instance)}"));
            var label = c.MarkLabel();
            c.GotoLabel(label, MoveType.Before);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<HoldSkyLeap, bool>>(self =>
            {
                if (Run.instance is InfiniteTowerRun run && run.waveController is InfiniteTowerWaveController waveController && waveController.spawnTarget is GameObject spawnTarget)
                {
                    self.characterMotor.Motor.SetPositionAndRotation(spawnTarget.transform.position + 2 * Vector3.up, Quaternion.identity, true);
                    return true;
                }

                return false;
            });
            c.Emit(OpCodes.Brfalse_S, label);
            c.Emit(OpCodes.Ret);
        }

        private static void SprintBash_FixedUpdate(ILCursor c)
        {
            c.GotoLast(MoveType.AfterLabel, x => x.MatchRet());
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<SprintBash>>(self =>
            {
                // TODO: should this be reserved for upgraded Mithrix?
                if (self.isAuthority && self.inputBank && self.skillLocator && self.skillLocator.secondary.IsReady() && self.inputBank.skill2.justPressed)
                {
                    self.skillLocator.secondary.ExecuteIfReady();
                }
            });
        }

        private static void FlyUpState_OnEnter(ILCursor c)
        {
            c.GotoNext(x => x.MatchLdsfld<FlyUpState>(nameof(FlyUpState.blastAttackDamageCoefficient)));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, FlyUpState, float>>((orig, self) => self.GetComponent<Mithrix.MithrixBodyBehavior>() ? 0 : orig);
        }

        private void GenericSkillHooks_IsSkillReady(GenericSkill skill, ref bool isReady)
        {
            if (skill.characterBody.name.Contains("Brother") && (skill.activationState.typeName.Contains("WeaponSlam") || skill.skillName == "Ult"))
            {
                isReady &= skill.characterBody.characterMotor.isGrounded || Physics.Raycast(skill.characterBody.transform.position, Vector3.down, 5, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
            }
        }

        private void BaseAIState_ModifyInputsForJumpIfNeccessary(On.EntityStates.AI.BaseAIState.orig_ModifyInputsForJumpIfNeccessary orig, EntityStates.AI.BaseAIState self, ref RoR2.CharacterAI.BaseAI.BodyInputs bodyInputs)
        {
            orig(self, ref bodyInputs);
            bodyInputs.pressJump |= IsMithrixAndWantsToHover();

            bool IsMithrixAndWantsToHover()
            {
                if (self.body.GetComponent<Mithrix.MithrixBodyBehavior>())
                {
                    return -self.bodyCharacterMotor.velocity.y > self.body.jumpPower;
                }

                return false;
            }
        }
    }
}