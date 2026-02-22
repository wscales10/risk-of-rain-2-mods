using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public abstract class BossFightBehavior : MonoBehaviour
    {
        protected CombatDirector CombatDirector { get; private set; }

        public virtual void Awake()
        {
            (this.CombatDirector = this.GetComponent<CombatDirector>()).AddSpawnListener(this.OnBossSpawnedServer);
            this.CombatDirector.combatSquad.onMemberDiscovered += this.OnCombatSquadMemberDiscovered;
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

        protected virtual void OnBossSpawnedServer(GameObject spawnedInstance)
        {
            var body = Utils.GetCharacterBody(spawnedInstance);

            if (body)
            {
                this.OnBossSpawnedServer(body!);
            }
            else
            {
                Debug.LogWarning("Spawned boss has no body.");
            }
        }

        protected abstract void OnBossSpawnedServer(CharacterBody body); // TODO could be abstract, but idk
    }
}