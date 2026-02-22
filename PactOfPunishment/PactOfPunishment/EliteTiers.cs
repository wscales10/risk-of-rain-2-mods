using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using RoR2;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace PactOfPunishment
{
    public class OverrideEliteTiersBehavior : MonoBehaviour
    {
        public CombatDirector.EliteTierDef[] eliteTiers = Array.Empty<CombatDirector.EliteTierDef>();
    }

    [RequireComponent(typeof(CombatDirector))]
    public class UseMinimumEliteTierBehavior : MonoBehaviour
    {
    }

    public class EliteTiers : Module
    {
        private EliteTiers()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public delegate int orig_get_mostExpensiveMonsterCostInDeck(CombatDirector self);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public delegate void hook_get_mostExpensiveMonsterCostInDeck(orig_get_mostExpensiveMonsterCostInDeck orig, CombatDirector self);

        public static EliteTiers Instance { get; } = new EliteTiers();

        public override void Init()
        {
            // Use minimum elite tier if desired
            IL.RoR2.CombatDirector.PrepareNewMonsterWave += Utils.HookIL(CombatDirector_PrepareNewMonsterWave);

            //IL.RoR2.CombatDirector.ResetEliteType += Utils.HookIL(this.ReplaceFieldGetInInstanceMethod);

            On.RoR2.CombatDirector.ResetEliteType += this.CombatDirector_ResetEliteType;

            //IL.RoR2.CombatDirector.PrepareNewMonsterWave += Utils.HookIL(this.ReplaceFieldGetInInstanceMethod);
            On.RoR2.CombatDirector.PrepareNewMonsterWave += this.CombatDirector_PrepareNewMonsterWave;
            HookEndpointManager.Modify<hook_get_mostExpensiveMonsterCostInDeck>(typeof(CombatDirector).GetProperty(nameof(CombatDirector.mostExpensiveMonsterCostInDeck), BindingFlags.Instance | BindingFlags.NonPublic).GetMethod, Utils.HookIL(this.CombatDirector_get_mostExpensiveMonsterCostInDeck));

            /* Not done:
             * RoR2.ClassicStageInfo.HandleSingleMonsterTypeArtifact, which uses CalcHighestEliteCostMultiplier
             * RoR2.Util.CreateReasonableDirectorCardSpawnList, which uses CalcHighestEliteCostMultiplier
             * RoR2.CombatDirector.lowestEliteCostMultiplier, which is not used, use GetLowestEliteCostMultiplier instead
             * Auto compatibility with other mods
             */
        }

        public CombatDirector.EliteTierDef[] GetEliteTiers(CombatDirector combatDirector)
        {
            if (combatDirector.TryGetComponent<OverrideEliteTiersBehavior>(out var behavior))
            {
                return behavior.eliteTiers;
            }

            return CombatDirector.eliteTiers;
        }

        /// <remarks>The static version of this method is not used anywhere in the vanilla codebase.</remarks>
        public float GetLowestEliteCostMultiplier(EliteIndex eliteIndex, CombatDirector combatDirector)
        {
            var eliteTiers = this.GetEliteTiers(combatDirector);

            for (int i = 1; i < eliteTiers.Length; i++)
            {
                CombatDirector.EliteTierDef? eliteTierDef = eliteTiers[i];
                for (int j = 0; j < eliteTierDef.eliteTypes.Length; j++)
                {
                    if (eliteTierDef.eliteTypes[j].eliteIndex == eliteIndex)
                    {
                        return eliteTierDef.costMultiplier;
                    }
                }
            }

            return CombatDirector.baseEliteCostMultiplier;
        }

        public float CalcHighestEliteCostMultiplier(SpawnCard.EliteRules eliteRules, CombatDirector combatDirector)
        {
            float num = 1f;
            var eliteTiers = this.GetEliteTiers(combatDirector);

            for (int i = 1; i < eliteTiers.Length; i++)
            {
                if (eliteTiers[i].CanSelect(eliteRules))
                {
                    num = Mathf.Max(num, eliteTiers[i].costMultiplier);
                }
            }

            return num;
        }

        private static void CombatDirector_PrepareNewMonsterWave(ILCursor c)
        {
            c.GotoNext(MoveType.After,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<CombatDirector>(nameof(CombatDirector.currentMonsterCard)),
                    x => x.MatchCallvirt<DirectorCard>(nameof(DirectorCard.GetSpawnCard)),
                    x => x.MatchIsinst<CharacterSpawnCard>(),
                    x => x.MatchLdfld<CharacterSpawnCard>(nameof(CharacterSpawnCard.noElites)),
                    x => x.MatchBrtrue(out _));
            c.Index--;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, CombatDirector, bool>>((orig, self) => orig || self.GetComponent<UseMinimumEliteTierBehavior>());
        }

        private void CombatDirector_PrepareNewMonsterWave(On.RoR2.CombatDirector.orig_PrepareNewMonsterWave orig, CombatDirector self, DirectorCard monsterCard)
        {
            var originalEliteTiers = CombatDirector.eliteTiers;
            CombatDirector.eliteTiers = this.GetEliteTiers(self);

            try
            {
                orig(self, monsterCard);
            }
            finally
            {
                CombatDirector.eliteTiers = originalEliteTiers;
            }
        }

        private void CombatDirector_ResetEliteType(On.RoR2.CombatDirector.orig_ResetEliteType orig, CombatDirector self)
        {
            var eliteTiers = this.GetEliteTiers(self);
            self.currentActiveEliteTier = eliteTiers[0];
            for (int i = 0; i < eliteTiers.Length; i++)
            {
                if (eliteTiers[i].CanSelect(self.currentMonsterCard.GetSpawnCard().eliteRules))
                {
                    self.currentActiveEliteTier = eliteTiers[i];
                    break;
                }
            }
            self.currentActiveEliteDef = self.currentActiveEliteTier.GetRandomAvailableEliteDef(self.rng);
        }

        private void CombatDirector_get_mostExpensiveMonsterCostInDeck(ILCursor c)
        {
            c.GotoNext(MoveType.AfterLabel, x => x.MatchCall<CombatDirector>(nameof(CombatDirector.CalcHighestEliteCostMultiplier)));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<SpawnCard.EliteRules, CombatDirector, float>>(this.CalcHighestEliteCostMultiplier);
        }

        private void ReplaceFieldGetInInstanceMethod(ILCursor c)
        {
            var eliteTiersVariable = new VariableDefinition(c.Context.Import(typeof(CombatDirector.EliteTierDef[])));
            c.Body.Variables.Add(eliteTiersVariable);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<CombatDirector, CombatDirector.EliteTierDef[]>>(this.GetEliteTiers);
            c.Emit(OpCodes.Stloc_S, (byte)eliteTiersVariable.Index);

            while (c.TryGotoNext(MoveType.AfterLabel, x => x.MatchLdsfld<CombatDirector>(nameof(CombatDirector.eliteTiers))))
            {
                c.Remove();
                c.Emit(OpCodes.Ldloc_S, (byte)eliteTiersVariable.Index);
            }
        }
    }
}