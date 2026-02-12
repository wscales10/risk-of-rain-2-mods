using PactOfPunishment.Conditions;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves
{
    public class ImpOverlord : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController> // TODO: custom reward drop table
    {
        private readonly CharacterSpawnCard impOverlordSpawnCard;

        public ImpOverlord()
        {
            this.impOverlordSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/ImpBoss/cscImpBoss.asset").WaitForCompletion();
        }

        protected override UpgradeWaveStrategy GetUpgradeMiniBossStrategy() => ScriptableObject.CreateInstance<PeriodicallySpawnGlacialJellyfish>();

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);

            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = this.impOverlordSpawnCard,
                }
            };
        }

        public class PeriodicallySpawnGlacialJellyfish : UpgradeWaveStrategy
        {
            private static readonly Lazy<DeployableSlot> deployableSlot = new Lazy<DeployableSlot>(() => DeployableAPI.RegisterDeployableSlot((_, __) => int.MaxValue));

            private readonly CharacterSpawnCard jellyfishSpawnCard;

            public PeriodicallySpawnGlacialJellyfish()
            {
                this.jellyfishSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Jellyfish/cscJellyfish.asset").WaitForCompletion();
            }

            public override void UpgradeWave(InfiniteTowerWaveController wave)
            {
                wave.combatDirector.onSpawnedServer ??= new CombatDirector.OnSpawnedServer();
                wave.combatDirector.onSpawnedServer.AddListener(spawnedEntity => this.OnBossSpawnedServer(wave, spawnedEntity));
            }

            private void OnBossSpawnedServer(InfiniteTowerWaveController wave, GameObject spawnedEntity)
            {
                var body = Utils.GetCharacterBody(spawnedEntity);
                if (!body || body!.bodyIndex != RoR2Content.BodyPrefabs.ImpBossBody.bodyIndex)
                {
                    return;
                }

                var behavior = body.gameObject.AddComponent<PeriodicallyDoSomething>();
                behavior.interval = 2;
                behavior.doSomething = () =>
                {
                    if (!wave.spawnTarget || !body || !body.master)
                    {
                        return;
                    }

                    DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Standard, out var minDistance, out var maxDistance);

                    var directorSpawnRequest = new DirectorSpawnRequest(this.jellyfishSpawnCard, new DirectorPlacementRule
                    {
                        minDistance = minDistance,
                        maxDistance = maxDistance,
                        position = wave.spawnTarget.transform.position,
                    }, RoR2Application.rng)
                    {
                        summonerBodyObject = body.gameObject,
                    };

                    var minion = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);

                    if (!minion)
                    {
                        return;
                    }

                    CharacterMaster minionMaster = minion.GetComponent<CharacterMaster>();
                    Inventory minionInventory = minion.GetComponent<Inventory>();
                    Utils.MakeUnscaledElite(minionInventory, RoR2Content.Elites.Ice);
                    var deployable = minion.AddComponent<Deployable>();
                    deployable.onUndeploy = new UnityEvent();
                    deployable.onUndeploy.AddListener(new UnityAction(minionMaster.TrueKill));
                    body.master.AddDeployable(deployable, deployableSlot.Value);
                };
            }
        }
    }
}