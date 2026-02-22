using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment
{
    public class MultiplyMaxHealthBehavior : MonoBehaviour
    {
        public readonly Dictionary<object, float> Multipliers = new Dictionary<object, float>();

        public void OnEnable()
        {
            RecalculateStats.Add(this.GetComponent<CharacterBody>(), this.MultiplyMaxHealth);
        }

        public void OnDisable()
        {
            RecalculateStats.Remove(this.GetComponent<CharacterBody>(), this.MultiplyMaxHealth);
        }

        private void MultiplyMaxHealth(RecalculateStatsAPI.StatHookEventArgs args)
        {
            foreach (var keyValuePair in this.Multipliers)
            {
                args.healthTotalMult *= keyValuePair.Value;
            }
        }
    }
}