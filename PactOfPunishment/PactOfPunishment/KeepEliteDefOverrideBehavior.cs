using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment
{
    public class KeepEliteDefOverride : Module
    {
        public override void Init()
        {
            IL.RoR2.CombatDirector.AttemptSpawnOnTarget += Utils.HookIL(CombatDirector_AttemptSpawnOnTarget);
        }

        private static void CombatDirector_AttemptSpawnOnTarget(ILCursor c)
        {
            while (c.TryGotoNext(
                x => x.MatchLdnull(),
                x => x.MatchStfld<CombatDirector>(nameof(CombatDirector.ActiveEliteDefOverride))))
            {
                c.RemoveRange(2);
                c.EmitDelegate<Action<CombatDirector>>((self) =>
                {
                    if (!self.TryGetComponent<KeepEliteDefOverrideBehavior>(out var behavior) || !behavior.HasCombatDirector(self))
                    {
                        self.ActiveEliteDefOverride = null;
                    }
                });
            }
        }
    }

    public class KeepEliteDefOverrideBehavior : MonoBehaviour
    {
        private readonly HashSet<CombatDirector> combatDirectors = new HashSet<CombatDirector>();

        public void AddCombatDirector(CombatDirector combatDirector)
        {
            this.combatDirectors.Add(combatDirector); // TODO: also remove when combat director dies, or in same context in which it's added?
        }

        public bool HasCombatDirector(CombatDirector combatDirector) => this.combatDirectors.Contains(combatDirector);
    }
}