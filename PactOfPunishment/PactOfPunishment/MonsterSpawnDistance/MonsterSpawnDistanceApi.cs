using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PactOfPunishment.MonsterSpawnDistance
{
    public static class MonsterSpawnDistanceApi
    {
        private static readonly Dictionary<int, Func<(float minimumDistance, float maximumDistance)>> registeredDistances = new Dictionary<int, Func<(float minimumDistance, float maximumDistance)>>();

        private static int maxKnownDistanceIndex = Enum.GetValues(typeof(DirectorCore.MonsterSpawnDistance)).Cast<int>().Max();

        public static DirectorCore.MonsterSpawnDistance RegisterMonsterSpawnDistance(Func<(float minimumDistance, float maximumDistance)> getMinimumMaximumDistance)
        {
            int index = ++maxKnownDistanceIndex;
            registeredDistances[index] = getMinimumMaximumDistance;
            return (DirectorCore.MonsterSpawnDistance)index;
        }

        internal static void TryGetCustomMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance input, ref float minimumDistance, ref float maximumDistance)
        {
            if (registeredDistances.TryGetValue((int)input, out var getMinimumMaximumDistance))
            {
                var distances = getMinimumMaximumDistance();
                minimumDistance = distances.minimumDistance;
                maximumDistance = distances.maximumDistance;
            }
        }
    }
}