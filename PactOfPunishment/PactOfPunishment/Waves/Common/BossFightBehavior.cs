using HG;
using PactOfPunishment.Conditions;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    [RequireComponent(typeof(CombatDirector))]
    public abstract class BossFightBehavior : MonoBehaviour
    {
        private EncounterContext? encounterContext;

        public CombatDirector CombatDirector { get; private set; }

        protected EncounterContext EncounterContext
        {
            get
            {
                if (this.encounterContext is null)
                {
                    var encounterContextHolder = this.GetComponent<EncounterContextHolder>();

                    if (!encounterContextHolder)
                    {
                        Debug.LogError($"{this} does not have an EncounterContextHolder");
                    }

                    this.encounterContext = encounterContextHolder.encounterContext;
                }

                return this.encounterContext;
            }
        }

        public virtual void Awake()
        {
            (this.CombatDirector = this.GetComponent<CombatDirector>()).AddSpawnListener(this.OnBossSpawnedServer);
            this.CombatDirector.combatSquad.onMemberDiscovered += this.OnCombatSquadMemberDiscovered;
            MonsterTracker.TrackCombatDirector(this.CombatDirector);
        }

        public virtual void OnEnable()
        {
            this.CombatDirector.EnsureComponent<InfiniteTowerWaveSpawnListener>().OnSpawnedServer += this.OnBossSpawnedServer;
        }

        public virtual void OnDisable()
        {
            this.CombatDirector.EnsureComponent<InfiniteTowerWaveSpawnListener>().OnSpawnedServer -= this.OnBossSpawnedServer;
        }

        protected virtual void OnCombatSquadMemberDiscovered(CharacterMaster master)
        {
            var body = master.GetBody();

            if (body)
            {
                this.OnCombatSquadMemberDiscovered(body);
            }
        }

        protected virtual void OnCombatSquadMemberDiscovered(CharacterBody body)
        {
        }

        protected virtual void OnBossSpawnedServer(SpawnCard.SpawnResult result)
        {
        }

        protected virtual void OnBossSpawnedServer(GameObject spawnedInstance)
        {
            if (Utils.TryGetCharacterBody(spawnedInstance, out var body))
            {
                this.OnBossSpawnedServer(body!);
            }
            else
            {
                Debug.LogWarning("Spawned boss has no body.");
            }
        }

        protected abstract void OnBossSpawnedServer(CharacterBody body);
    }
}