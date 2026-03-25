using HG;
using PactOfPunishment.Waves.Infrastructure;
using RoR2;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public interface IPortableMiniBossWaveDefinition : ISimulacrumWaveDefinition
    {
        IPortableMiniBossInfo[] MiniBosses { get; }

        void EnsureGameObjectHasBossFightBehavior(GameObject miniBossCombatDirectorGameObject);
    }

    public abstract class MiniBossWaveDefinition<TWaveController> : SimulacrumWaveDefinition<TWaveController> where TWaveController : InfiniteTowerWaveController
    {

        protected override ItemTier RewardDisplayTier => ItemTier.Tier2;

        protected static BasicPickupDropTable GetBaseDropTable(Run run)
        {
            return BossDropTables.Instance.GetRare(run);
        }

        protected override PickupDropTable GetRewardDropTable(Run run)
        {
            return GetBaseDropTable(run);
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, TWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.secondsBeforeSuddenDeath *= 1.5f;
            wavePrefab.suddenDeathRadiusConstrictingPerSecond /= 1.5f;
        }
    }

    public abstract class PortableMiniBossWaveDefinition<T> : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>, IPortableMiniBossWaveDefinition
        where T : PortableMiniBossFightBehavior<T>
    {
        private readonly PortableMiniBossInfo<T>[] miniBosses;

        protected PortableMiniBossWaveDefinition(params PortableMiniBossInfo<T>[] miniBosses)
        {
            this.miniBosses = miniBosses;
        }

        IPortableMiniBossInfo[] IPortableMiniBossWaveDefinition.MiniBosses => this.miniBosses;

        public void EnsureGameObjectHasBossFightBehavior(GameObject miniBossCombatDirectorGameObject)
        {
            miniBossCombatDirectorGameObject.EnsureComponent<T>().SetMiniBosses(this.miniBosses);
            this.TryAddUpgradeBehavior(miniBossCombatDirectorGameObject);
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.spawnList = this.miniBosses.Select(x => x.SpawnInfo).ToArray();
            dir.EnsureComponent<T>().SetMiniBosses(this.miniBosses);
        }
    }

    public abstract class PortableMiniBossInfo : ScriptableObject, IPortableMiniBossInfo
    {
        public abstract InfiniteTowerExplicitSpawnWaveController.SpawnInfo SpawnInfo { get; }

        public virtual float RelativePowerLevel => 1;

        public CharacterBody BodyPrefab => this.SpawnInfo.spawnCard.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<CharacterBody>();

        public abstract void SetupBossBody(CharacterBody body, BossFightBehavior bossFightBehavior);
    }

    public abstract class PortableMiniBossInfo<T> : PortableMiniBossInfo
        where T : PortableMiniBossFightBehavior<T>
    {
        public sealed override void SetupBossBody(CharacterBody body, BossFightBehavior bossFightBehavior) => this.SetupBossBody(body, (T)bossFightBehavior);

        public abstract void SetupBossBody(CharacterBody body, T bossFightBehavior);
    }
}