using PactOfPunishment.Conditions;
using RoR2;
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

        public int GetRank(IConditionDef conditionDef)
        {
            return this.rank.Get(conditionDef.GetType());
        }

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