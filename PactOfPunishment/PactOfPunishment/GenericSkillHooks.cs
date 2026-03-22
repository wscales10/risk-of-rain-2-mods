using RoR2;
using System;

namespace PactOfPunishment
{
    public class GenericSkillHooks : Module
    {
        public delegate void SkillIsReadyEventHandler(GenericSkill skill, ref bool isReady);

        public static event SkillIsReadyEventHandler? IsSkillReady;

        public override void Init()
        {
            On.RoR2.GenericSkill.IsReady += this.GenericSkill_IsReady;
        }

        private bool GenericSkill_IsReady(On.RoR2.GenericSkill.orig_IsReady orig, GenericSkill self)
        {
            bool result = orig(self);

            if (!result)
            {
                return false;
            }

            foreach (var del in Utils.GetInvocationList(IsSkillReady))
            {
                try
                {
                    del(self, ref result);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex);
                }

                if (!result)
                {
                    return false;
                }
            }

            return true;
        }
    }
}