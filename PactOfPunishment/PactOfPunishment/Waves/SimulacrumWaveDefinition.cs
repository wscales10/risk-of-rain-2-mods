using HG;
using PactOfPunishment.Conditions;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment.Waves
{
    public abstract class SimulacrumWaveDefinition<TWaveController> where TWaveController : InfiniteTowerWaveController
    {
        private readonly Lazy<GameObject> baseWavePrefab;
        protected static readonly Lazy<GameObject> defaultBossWavePrefab = new Lazy<GameObject>(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveBoss.prefab").WaitForCompletion);

        protected SimulacrumWaveDefinition()
        {
            this.baseWavePrefab = new Lazy<GameObject>(() => Addressables.LoadAssetAsync<GameObject>(this.BaseWavePrefabKey).WaitForCompletion()); // TODO: does it need to be lazy?
        }

        public virtual string Name => this.GetType().Name;

        protected abstract ItemTier RewardDisplayTier { get; }

        protected virtual string BaseWavePrefabKey => "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveBase.prefab";

        public GameObject? MakeWavePrefab(Run run)
        {
            try
            {
                var prefab = PrefabAPI.InstantiateClone(this.baseWavePrefab.Value, "InfiniteTowerWaveBoss" + this.Name);
                CombatDirector dir = prefab.EnsureComponent<CombatDirector>();
                CombatSquad squad = prefab.EnsureComponent<CombatSquad>();

                TWaveController wavePrefab = prefab.EnsureComponent<TWaveController>();
                wavePrefab.rewardDropTable = this.GetRewardDropTable(run);
                wavePrefab.rewardDisplayTier = this.RewardDisplayTier;

                this.Setup(dir, squad, wavePrefab);

                var upgradeStrategy = this.GetUpgradeStrategy();
                if (upgradeStrategy)
                {
                    prefab.AddComponent<UpgradeWaveBehavior>().strategy = upgradeStrategy;
                }

                return prefab;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        protected abstract PickupDropTable GetRewardDropTable(Run run);

        protected virtual void Setup(CombatDirector dir, CombatSquad squad, TWaveController wavePrefab)
        {
            wavePrefab.combatDirector = dir;
            wavePrefab.combatSquad = squad;
            wavePrefab.isBossWave = true;

            var defaultBossWave = defaultBossWavePrefab.Value.GetComponent<InfiniteTowerWaveController>();
            wavePrefab.rewardPickupPrefab = defaultBossWave.rewardPickupPrefab;
            wavePrefab.secondsAfterWave = defaultBossWave.secondsAfterWave;
            wavePrefab.uiPrefab = defaultBossWave.uiPrefab;
            wavePrefab.overlayEntries = defaultBossWave.overlayEntries;
            wavePrefab.rewardOptionCount = defaultBossWave.rewardOptionCount;
            wavePrefab.rewardOffset = defaultBossWave.rewardOffset;
            wavePrefab.beginSoundString = defaultBossWave.beginSoundString;
            wavePrefab.onAllEnemiesDefeatedSoundString = defaultBossWave.onAllEnemiesDefeatedSoundString;
        }

        protected abstract UpgradeWaveStrategy GetUpgradeStrategy();
    }
}