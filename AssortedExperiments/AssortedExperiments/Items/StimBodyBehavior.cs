using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AssortedExperiments.Items
{
    public class StimBodyBehavior : BaseItemBodyBehavior
    {
        private readonly List<HealingEvent> events = new List<HealingEvent>();

        private readonly List<TargetRecord> targetRecords = new List<TargetRecord>();

        private float healingEventLifetime = 0.5f;

        private float cap = 0.5f;

        // If true, caps by rate, otherwise caps by sum
        private bool isRateCap = false;

        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return Content.Items.Stim;
        }

        public void ProcessHeal(ref float amount, float fullCombinedHealth)
        {
            float healMultiplier = Mathf.Pow(0.6f, this.stack);

            // TODO: in early stages especially, this would produce a good synergy between "flat" healing items like planula/medkit/monster tooth and max health reduction like shaped glass
            // Consider how you want this item to interact with items which significantly lower or
            // increase your maximum health
            if (amount > 0 && fullCombinedHealth > 0)
            {
                float stimMultiplier = 0.2f + 0.8f * (1 - healMultiplier);
                this.events.Add(new HealingEvent(amount / fullCombinedHealth, stimMultiplier, Run.instance.time));
            }

            amount *= healMultiplier;
        }

        private static IEnumerable<T> RemoveWhereYieldOthers<T>(List<T> list, Func<T, bool> shouldRemove)
        {
            int i = 0;

            while (i < list.Count)
            {
                var current = list[i];

                if (shouldRemove(current))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    yield return current;
                    i++;
                }
            }
        }

        private void OnDisable()
        {
            this.events.Clear();
            this.targetRecords.Clear();
            this.SetBuffCount(0);
        }

        private void Update()
        {
            var currentTime = Run.instance.time;
            var cutoffTime = currentTime - this.healingEventLifetime;

            float sum = 0f;
            float fullSum = 0f;

            foreach (var healingEvent in RemoveWhereYieldOthers(this.events, healingEvent => healingEvent.Time < cutoffTime))
            {
                sum += healingEvent.FullAmount * healingEvent.StimMultiplier;
                fullSum += healingEvent.FullAmount;
            }

            float targetBuffPercentage = Mathf.Clamp01(sum / (this.isRateCap ? this.healingEventLifetime * this.cap : this.cap)) * 100;

            int i = 0;
            while (i < this.targetRecords.Count)
            {
                var targetRecord = this.targetRecords[i];

                if (targetRecord.Time < cutoffTime)
                {
                    this.targetRecords.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            this.targetRecords.Add(new TargetRecord(targetBuffPercentage, currentTime));

            int buffCount = (int)Mathf.Round(this.targetRecords.Select(x => x.TargetBuffPercentage).Max());
            Debug.LogFormat("Current average HPS over max health: {0}. Setting buff count {1}.", Math.Round(fullSum / this.healingEventLifetime, 4), buffCount);

            // TODO: Take into account Growth Nectar - does 0-2% really count as a significant buff? Should Growth Nectar ignore this?
            this.SetBuffCount(buffCount);
        }

        private void SetBuffCount(int buffCount)
        {
            this.body.SetBuffCount(Content.Buffs.Stim.buffIndex, buffCount);
        }

        private readonly struct HealingEvent
        {
            public HealingEvent(float fullAmount, float stimMultiplier, float time)
            {
                this.FullAmount = fullAmount;
                this.StimMultiplier = stimMultiplier;
                this.Time = time;
            }

            public float FullAmount { get; }

            public float StimMultiplier { get; }

            public float Time { get; }

            public override string ToString()
            {
                return $"t: {this.Time} / %: {this.FullAmount} / m: {this.StimMultiplier}";
            }
        }

        private readonly struct TargetRecord
        {
            public TargetRecord(float targetBuffPercentage, float time)
            {
                this.TargetBuffPercentage = targetBuffPercentage;
                this.Time = time;
            }

            public float TargetBuffPercentage { get; }

            public float Time { get; }

            public override string ToString()
            {
                return $"t: {this.Time} / %: {this.TargetBuffPercentage}";
            }
        }
    }
}