using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment
{
    public class MultiplySkillCooldownsBehavior : MonoBehaviour
    {
        public readonly Dictionary<object, float> Multipliers = new Dictionary<object, float>();

        public void OnEnable()
        {
            RecalculateStats.Add(this.GetComponent<CharacterBody>(), this.MultiplySkillCooldowns);
        }

        public void OnDisable()
        {
            RecalculateStats.Remove(this.GetComponent<CharacterBody>(), this.MultiplySkillCooldowns);
        }

        private void MultiplySkillCooldowns(RecalculateStatsAPI.StatHookEventArgs args)
        {
            foreach (var keyValuePair in this.Multipliers)
            {
                args.allSkills.cooldownMultiplier *= keyValuePair.Value;
            }
        }
    }
}