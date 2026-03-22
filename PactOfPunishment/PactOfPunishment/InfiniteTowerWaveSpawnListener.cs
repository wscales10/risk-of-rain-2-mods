using PactOfPunishment.Conditions;
using RoR2;
using RoR2.Artifacts;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public class InfiniteTowerWaveSpawnListener : MonoBehaviour
    {
        private CombatDirector combatDirector;

        public event Action<SpawnCard.SpawnResult>? OnSpawnedServer;

        public void Awake()
        {
            this.combatDirector = this.GetComponent<CombatDirector>();
        }

        public void OnEnable()
        {
            SpawnCard.onSpawnedServerGlobal += this.SpawnCard_onSpawnedServerGlobal;
        }

        public void OnDisable()
        {
            SpawnCard.onSpawnedServerGlobal -= this.SpawnCard_onSpawnedServerGlobal;
        }

        private void SpawnCard_onSpawnedServerGlobal(SpawnCard.SpawnResult result)
        {
            if (!MonsterTracker.Match(this.combatDirector, result))
            {
                return;
            }

            GameObject spawnedInstance = result.spawnedInstance;

            if (spawnedInstance)
            {
                CharacterMaster spawnedMaster = spawnedInstance.GetComponent<CharacterMaster>();

                if (spawnedMaster.inventory.currentEquipmentIndex == EquipmentIndex.None)
                {
                    EliteOnlyArtifactManager.PromoteIfHonor(spawnedMaster, this.combatDirector.rng, this.combatDirector.GetEliteDefsFromCheapestAvailableTier(result.spawnRequest.spawnCard).ToArray());
                }
            }

            this.OnSpawnedServer?.Invoke(result);
        }
    }
}