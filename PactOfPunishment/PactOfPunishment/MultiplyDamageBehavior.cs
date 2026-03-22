using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment
{
    public class MultiplyDamageBehavior : MonoBehaviour
    {
        public readonly Dictionary<object, float> Multipliers = new Dictionary<object, float>();

        public void OnEnable()
        {
            RecalculateStats.Add(this.GetComponent<CharacterBody>(), this.MultiplyDamage);
        }

        public void OnDisable()
        {
            RecalculateStats.Remove(this.GetComponent<CharacterBody>(), this.MultiplyDamage);
        }

        private void MultiplyDamage(RecalculateStatsAPI.StatHookEventArgs args)
        {
            foreach (var keyValuePair in this.Multipliers)
            {
                args.damageTotalMult *= keyValuePair.Value;
            }
        }
    }
}