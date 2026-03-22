using PactOfPunishment.Conditions;
using RoR2;
using RoR2.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public class PactOfPunishmentBehavior : MonoBehaviour // TODO: support ProperSave?
    {
        private readonly HashSet<Condition> activeConditions = new HashSet<Condition>(new ConditionEqualityComparer());

        private LazyRankLookup rank;

        public static PactOfPunishmentBehavior Instance => Run.instance.GetComponent<PactOfPunishmentBehavior>();

        public void SetConditions(IEnumerable<Condition> conditions) // TODO: use an event listener to get conditions so other mods could theoretically add their own
        {
            this.activeConditions.Clear();

            foreach (Condition condition in conditions.Where(x => x.Rank > 0))
            {
                this.activeConditions.Add(condition);
            }
        }

        public void OnRunVictory()
        {
            var statValue = this.activeConditions.Sum(x => x.TotalHeat);

            foreach (PlayerStatsComponent instance in PlayerStatsComponent.instancesList)
            {
                if (!instance.playerCharacterMasterController.isConnected)
                {
                    continue;
                }

                PerBodyStatDef? perBodyStatDef = null;
                switch (Run.instance.selectedDifficulty)
                {
                    case DifficultyIndex.Easy:
                        perBodyStatDef = Content.StatDefs.PerBodyHeatEasy;
                        break;
                    case DifficultyIndex.Normal:
                        perBodyStatDef = Content.StatDefs.PerBodyHeatNormal;
                        break;
                    case DifficultyIndex.Hard:
                        perBodyStatDef = Content.StatDefs.PerBodyHeatHard;
                        break;
                }
                StatSheet currentStats = instance.currentStats;
                currentStats.PushStatValue(Content.StatDefs.Heat, statValue);
                if (perBodyStatDef != null)
                {
                    CharacterBody body = instance.characterMaster.GetBody();

                    if (body)
                    {
                        string bodyName = BodyCatalog.GetBodyName(body.bodyIndex);
                        currentStats.PushStatValue(perBodyStatDef.FindStatDef(bodyName ?? ""), statValue);
                    }
                }
            }
        }

        public int GetRank(IConditionDef conditionDef)
        {
            return this.rank.Get(conditionDef.GetType());
        }

        public int TotalHeat => this.activeConditions.Sum(x => x.TotalHeat);

        private void Awake()
        {
            this.rank = new LazyRankLookup(this.GetRankInternal);
        }

        private int GetRankInternal(Type t)
        {
            return this.activeConditions.Where(x => t.IsInstanceOfType(x.ConditionDef)).Sum(x => x.Rank);
        }

        private sealed class LazyRankLookup
        {
            private readonly Dictionary<Type, int> dictionary = new Dictionary<Type, int>();

            private readonly Func<Type, int> getRank;

            public LazyRankLookup(Func<Type, int> getRank)
            {
                this.getRank = getRank;
            }

            public int Get(Type type)
            {
                if (!this.dictionary.TryGetValue(type, out var rank))
                {
                    this.dictionary[type] = rank = this.getRank(type);
                }

                return rank;
            }

            public void Forget()
            {
                this.dictionary.Clear();
            }
        }
    }
}