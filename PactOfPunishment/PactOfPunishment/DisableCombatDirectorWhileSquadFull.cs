using RoR2;
using UnityEngine;

namespace PactOfPunishment
{
    [RequireComponent(typeof(CombatDirector))]
    public class DisableWhileSquadFullBehavior : MonoBehaviour
    {
    }

    public class DisableCombatDirectorWhileSquadFull : Module
    {
        public override void Init()
        {
            On.RoR2.CombatDirector.FixedUpdate += CombatDirector_FixedUpdate;
        }

        private static void CombatDirector_FixedUpdate(On.RoR2.CombatDirector.orig_FixedUpdate orig, RoR2.CombatDirector self)
        {
            if (self.GetComponent<DisableWhileSquadFullBehavior>())
            {
                var maxSquadCount = ScaleMaxSquadCount.GetMaxSquadCount(self);

                if ((self.combatSquad && maxSquadCount != 0 && self.combatSquad.memberCount >= maxSquadCount))
                {
                    return;
                }
            }

            orig(self);
        }
    }
}