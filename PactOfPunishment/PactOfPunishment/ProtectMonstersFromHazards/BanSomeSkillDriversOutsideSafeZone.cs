using RoR2.CharacterAI;

namespace PactOfPunishment.ProtectMonstersFromHazards
{
    public class BanSomeSkillDriversOutsideSafeZone : Module // TODO: this still doesn't stop Halcyonite3 (and others I suspect) from getting stuck outside the safe zone and getting all their health drained.
    {
        public override void Init()
        {
            On.RoR2.CharacterAI.BaseAI.EvaluateSingleSkillDriver += this.BaseAI_EvaluateSingleSkillDriver;
        }

        private BaseAI.SkillDriverEvaluation? BaseAI_EvaluateSingleSkillDriver(On.RoR2.CharacterAI.BaseAI.orig_EvaluateSingleSkillDriver orig, BaseAI self, ref BaseAI.SkillDriverEvaluation currentSkillDriverEvaluation, AISkillDriver aiSkillDriver, float myHealthFraction)
        {
            var originalResult = orig(self, ref currentSkillDriverEvaluation, aiSkillDriver, myHealthFraction);

            if (originalResult is null || Utils.IsSafeLocation(self.body.teamComponent.transform.position) || aiSkillDriver.moveInputScale > 0 && aiSkillDriver.movementType != AISkillDriver.MovementType.Stop)
            {
                return originalResult;
            }

            return null;
        }
    }
}