using HG;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public class MonsterTracker : MonoBehaviour
    {
        public CombatDirector? combatDirector;

        public static void TrackCombatDirector(CombatDirector combatDirector)
        {
            combatDirector.onSpawnedWithDirectorServer.AddListener(OnSpawnedWithDirectorServer); // TODO: remove this listener at some point?
        }

        public static bool Match(CombatDirector combatDirector, SpawnCard.SpawnResult spawnResult)
        {
            return spawnResult.success && spawnResult.spawnedInstance && spawnResult.spawnedInstance.TryGetComponent<MonsterTracker>(out var tracker) && tracker.combatDirector == combatDirector;
        }

        private static void OnSpawnedWithDirectorServer(GameObject spawnedEntity, CombatDirector combatDirector)
        {
            var tracker = spawnedEntity.EnsureComponent<MonsterTracker>();
            tracker.combatDirector = combatDirector;
        }
    }
}