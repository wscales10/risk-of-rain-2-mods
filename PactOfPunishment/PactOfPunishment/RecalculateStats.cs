using R2API;
using RoR2;
using System;
using System.Collections.Generic;

namespace PactOfPunishment
{
    public class RecalculateStats : Module
    {
        private static readonly Dictionary<CharacterBody, List<Action<RecalculateStatsAPI.StatHookEventArgs>>> dictionary = new Dictionary<CharacterBody, List<Action<RecalculateStatsAPI.StatHookEventArgs>>>();

        private RecalculateStats()
        {
        }

        public static RecalculateStats Instance { get; } = new RecalculateStats();

        public static void Add(CharacterBody body, Action<RecalculateStatsAPI.StatHookEventArgs> action)
        {
            if (!dictionary.TryGetValue(body, out var set))
            {
                set = new List<Action<RecalculateStatsAPI.StatHookEventArgs>>();
                dictionary.Add(body, set);
            }

            if (!set.Contains(action))
            {
                set.Add(action);
            }
        }

        public static void Remove(CharacterBody body, Action<RecalculateStatsAPI.StatHookEventArgs> action)
        {
            if (!dictionary.TryGetValue(body, out var set))
            {
                return;
            }

            set.Remove(action);
        }

        public override void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (dictionary.TryGetValue(sender, out var list))
            {
                foreach (var action in list)
                {
                    action(args);
                }
            }
        }
    }
}