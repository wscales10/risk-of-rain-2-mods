using RoR2.Skills;
using System;

namespace PactOfPunishment.BugFixes
{
    /// <remarks>
    /// This module changes the False Son Boss's Lunar Gaze skill to use the Body state machine
    /// rather than the Weapon state machine, so that it cannot be inadvertently interrupted by
    /// other skills.
    /// </remarks>
    public class FalseSonBossLunarGazeInterrupt : Module
    {
        public override void Init()
        {
            SkillCatalog.skillsDefined.CallWhenAvailable(() =>
            {
                int lunarGazeSkillIndex = Array.IndexOf(SkillCatalog._allSkillNames, "FalseSonBossLunarGaze");

                // If lunarGazeSkillIndex is -1, then I want this to throw an exception so that it
                // is clear from the logs that the module was not applied successfully.
                SkillCatalog._allSkillDefs[lunarGazeSkillIndex].activationStateMachineName = "Body";
            });
        }
    }
}