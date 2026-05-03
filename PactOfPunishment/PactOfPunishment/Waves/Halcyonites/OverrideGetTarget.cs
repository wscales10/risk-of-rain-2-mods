using EntityStates.Halcyonite;
using RoR2.CharacterAI;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment.Waves.Halcyonites
{
    public partial class WhirlWindModule
    {
        public class OverrideGetTarget : OverrideGetTarget<WhirlWindTargetInfo, WhirlWindPersuitCycle>, IOverrideHasTarget<bool, WhirlWindPersuitCycle>
        {
            public bool GetResult(bool orig, WhirlWindPersuitCycle args)
            {
                return orig || args.targetPos != default;
            }

            protected override IEnumerable<OrigResult<WhirlWindTargetInfo>> Foo(WhirlWindPersuitCycle args)
            {
                if (args.characterBody.master.TryGetComponent<BaseAI>(out var ai) && ai.skillDriverEvaluation.dominantSkillDriver.requiredSkill == HalcyoniteModule.WhirlwindSkillDef && ai.skillDriverEvaluation.target.gameObject)
                {
                    if (ai.skillDriverEvaluation.target.lastKnownBullseyePosition == null)
                    {
                        var targetBody = ai.skillDriverEvaluation.target.characterBody;

                        if (!targetBody)
                        {
                            this.Result = new WhirlWindTargetInfo
                            {
                                body = null,
                                pos = GetTargetPosition(ai.skillDriverEvaluation.target.gameObject.transform.position)
                            };
                            yield break;
                        }
                    }
                    else
                    {
                        this.Result = new WhirlWindTargetInfo
                        {
                            body = ai.skillDriverEvaluation.target.characterBody,
                            pos = GetTargetPosition(ai.skillDriverEvaluation.target.lastKnownBullseyePosition.Value)
                        };
                        yield break;
                    }
                }

                yield return this.Orig(out var orig);
                /*if (orig.Value.body == null)
                {
                    if (!Utils.IsSafeLocation(args.characterBody.corePosition) && Run.instance is InfiniteTowerRun run && run.safeWardController)
                    {
                        this.Result = new WhirlWindTargetInfo
                        {
                            pos = GetTargetPosition(run.safeWardController.transform.position)
                        };
                        yield break;
                    }

                    if (args.characterBody.master.TryGetComponent<BaseAI>(out ai) && ai.currentEnemy?.lastKnownBullseyePosition != null)
                    {
                        this.Result = new WhirlWindTargetInfo
                        {
                            body = ai.currentEnemy!.characterBody,
                            pos = GetTargetPosition(ai.currentEnemy.lastKnownBullseyePosition.Value),
                        };
                        yield break;
                    }
                }*/

                this.Result = orig.Value;

                Vector3 GetTargetPosition(Vector3 position)
                {
                    return position + (args.transform.position - position).normalized * 2f;
                }
            }
        }
    }
}