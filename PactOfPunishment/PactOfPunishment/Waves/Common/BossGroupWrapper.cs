using HG;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public static class BossGroupExtensions
    {
        public static void AddBossToGroup(this Component owner, ref BossGroupWrapper? wrapper, CharacterBody body)
        {
            wrapper ??= owner.gameObject.AddComponent<BossGroupWrapper>();
            wrapper.CombatSquad.AddMember(body.master);
        }
    }

    public class BossGroupWrapper : MonoBehaviour
    {
        private readonly BossGroup bossGroup;

        private readonly GameObject bossGroupGameObject;

        public BossGroupWrapper()
        {
            this.bossGroupGameObject = new GameObject();
            this.bossGroupGameObject.transform.SetParent(this.gameObject.transform, false);
            this.CombatSquad = this.bossGroupGameObject.EnsureComponent<CombatSquad>();
            this.bossGroup = this.bossGroupGameObject.EnsureComponent<BossGroup>();
        }

        public CombatSquad CombatSquad { get; }

        public void OnDestroy()
        {
            Destroy(this.bossGroupGameObject);
        }
    }
}