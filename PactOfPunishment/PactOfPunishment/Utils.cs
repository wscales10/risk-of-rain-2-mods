using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment
{
    public static class Utils
    {
        public static bool IsFoe(CharacterBody characterBody)
        {
            if (!characterBody)
            {
                return false;
            }

            var teamComponent = characterBody.teamComponent;
            return teamComponent && TeamManager.IsTeamEnemy(TeamIndex.Player, teamComponent.teamIndex);
        }

        public static BuffDef AddStatusEffect(Action<BuffDef> setup)
        {
            var buffDef = ScriptableObject.CreateInstance<BuffDef>();
            setup?.Invoke(buffDef);

            if (!ContentAddition.AddBuffDef(buffDef))
            {
                throw new InvalidOperationException();
            }

            return buffDef;
        }

        public static ILContext.Manipulator HookIL(this Action<ILCursor> hook)
        {
            return il =>
            {
                var c = new ILCursor(il);
                hook(c);
            };
        }

        public static IEnumerable<EliteDef> GetEliteDefs(SpawnCard spawnCard)
        {
            return CombatDirector.eliteTiers.Where(tier => tier.CanSelect(spawnCard.eliteRules)).SelectMany(tier => tier.eliteTypes.Where(elite => elite && elite.IsAvailable()));
        }

        public static void RemoveWhere<T>(this WeightedSelection<T> weightedSelection, Func<T, bool> predicate)
        {
            for (int i = weightedSelection.Count - 1 - 1; i >= 0; i--)
            {
                var choice = weightedSelection.GetChoice(i);

                if (predicate(choice.value))
                {
                    weightedSelection.RemoveChoice(i);
                }
            }
        }

        public static void AddChoicesWithRelativeWeight<T>(this WeightedSelection<T> weightedSelection, float weightOfOriginalSelection, Func<T, bool> predicate, params (T value, float weight)[] choices)
        {
            float totalWeight = weightedSelection.getTotalWeight();
            float weightMultiplier;

            if (weightOfOriginalSelection <= 0)
            {
                weightedSelection.Clear();
                weightMultiplier = 1;
            }
            else if (Mathf.Approximately(totalWeight, 0))
            {
                weightMultiplier = 1;
            }
            else
            {
                weightMultiplier = totalWeight / weightOfOriginalSelection;
            }

            foreach (var (value, weight) in choices.Where(x => predicate?.Invoke(x.value) != false))
            {
                weightedSelection.AddChoice(value, weight * weightMultiplier);
            }
        }

        public static void DisableSkill(CharacterBody body, Func<SkillLocator, GenericSkill> getSkill)
        {
            var skill = getSkill(body.skillLocator);

            if (skill)
            {
                skill.SetSkillOverride(body, CharacterBody.CommonAssets.disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public static CharacterBody? GetCharacterBody(GameObject entity)
        {
            if (entity.TryGetComponent<CharacterMaster>(out var master))
            {
                return master.GetBody();
            }

            return null;
        }
    }
}