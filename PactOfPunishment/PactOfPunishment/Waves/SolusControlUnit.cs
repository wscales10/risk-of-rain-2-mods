using PactOfPunishment.Conditions;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves
{
    public sealed class SolusControlUnit : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private static readonly Lazy<DeployableSlot> deployableSlot = new Lazy<DeployableSlot>(() => DeployableAPI.RegisterDeployableSlot((_, __) => int.MaxValue));

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/RoboBallBoss/cscRoboBallBoss.asset").WaitForCompletion()
                }
            };

            dir.gameObject.AddComponent<SolusControlUnitBossBehavior>();
        }

        protected override PickupDropTable GetRewardDropTable(Run run)
        {
            var baseDropTable = GetBaseDropTable(run);
            var dropTable = ScriptableObject.CreateInstance<BetterExplicitPickupDropTable>();
            int count = baseDropTable.GetPickupCount();
            float totalBossWeight = 0, totalCoresWeight = 0;
            var pickupEntries = new List<BetterExplicitPickupDropTable.PickupIndexEntry>();

            for (int i = 0; i < count; i++)
            {
                var current = baseDropTable.selector.GetChoice(i);
                var pickupDef = PickupCatalog.GetPickupDef(current.value.pickupIndex);

                if (pickupDef.itemTier == ItemTier.Boss)
                {
                    totalBossWeight += current.weight;
                }

                if (pickupDef.itemIndex == RoR2Content.Items.RoboBallBuddy.itemIndex)
                {
                    totalCoresWeight += current.weight;
                }
            }

            bool foundCores = !Mathf.Approximately(totalCoresWeight, 0);

            for (int i = 0; i < count; i++)
            {
                var current = baseDropTable.selector.GetChoice(i);
                var pickupDef = PickupCatalog.GetPickupDef(current.value.pickupIndex);
                float adjustedWeight = current.weight;

                if (foundCores)
                {
                    if (pickupDef.itemIndex == RoR2Content.Items.RoboBallBuddy.itemIndex)
                    {
                        adjustedWeight *= totalBossWeight / totalCoresWeight;
                    }
                    else if (pickupDef.itemTier == ItemTier.Boss)
                    {
                        continue;
                    }
                }

                pickupEntries.Add(new BetterExplicitPickupDropTable.PickupIndexEntry { pickupIndex = pickupDef.pickupIndex, pickupWeight = adjustedWeight });
            }

            dropTable.pickupEntries = pickupEntries.ToArray();
            return dropTable;
        }

        protected override UpgradeWaveStrategy GetUpgradeMiniBossStrategy() => ScriptableObject.CreateInstance<NullUpgradeStrategy>(); // TODO: add upgrade strategy

        public class SolusControlUnitBossBehavior : MonoBehaviour
        {
            private static readonly Lazy<CharacterSpawnCard> solusProbeSpawnCard = GetLazySpawnCard("RoR2/Base/RoboBallBoss/cscRoboBallMini.asset");

            public void Awake()
            {
                var dir = this.GetComponent<CombatDirector>();
                dir.onSpawnedServer ??= new CombatDirector.OnSpawnedServer();
                dir.onSpawnedServer.AddListener(this.OnBossSpawnedServer);
            }

            private static void OnTakeNonZeroDamageGlobal(HealthComponent victim, CharacterBody body)
            {
                if (victim == body.healthComponent)
                {
                    victim.GetComponent<RateLimiter>().TryDoThing();
                }
            }

            private void OnBossSpawnedServer(GameObject spawnedEntity)
            {
                var body = Utils.GetCharacterBody(spawnedEntity);
                if (!body || body!.bodyIndex != RoR2Content.BodyPrefabs.RoboBallBossBody.bodyIndex)
                {
                    return;
                }

                Utils.DisableSkill(body, x => x.special);
                var spawnMinionsBehavior = body.gameObject.AddComponent<RateLimiter>();
                spawnMinionsBehavior.doThing = () =>
                {
                    DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Close, out var minDistance, out var maxDistance);
                    var directorSpawnRequest = new DirectorSpawnRequest(solusProbeSpawnCard.Value, new DirectorPlacementRule
                    {
                        minDistance = minDistance,
                        maxDistance = maxDistance,
                        position = body.corePosition,
                    }, RoR2Application.rng)
                    {
                        summonerBodyObject = body.gameObject
                    };

                    var probe = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);

                    if (!probe)
                    {
                        return false;
                    }

                    CharacterMaster probeMaster = probe.GetComponent<CharacterMaster>();
                    Inventory probeInventory = probe.GetComponent<Inventory>();
                    probeInventory.SetEquipmentIndex(body.inventory.currentEquipmentIndex, false);

                    if (body.inventory.GetItemCountEffective(RoR2Content.Items.Ghost) > 0)
                    {
                        probeInventory.GiveItemPermanent(RoR2Content.Items.Ghost);
                        probeInventory.GiveItemPermanent(RoR2Content.Items.HealthDecay, 30);
                        probeInventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, 150);
                    }

                    probeInventory.GiveItemPermanent(RoR2Content.Items.CutHp, 2);

                    var deployable = probe.AddComponent<Deployable>();
                    deployable.onUndeploy = new UnityEvent();
                    deployable.onUndeploy.AddListener(new UnityAction(probeMaster.TrueKill));
                    body.master.AddDeployable(deployable, deployableSlot.Value);
                    return true;
                };
                SimulacrumWaves.OnTakeNonZeroDamageGlobal += (victim, _) => OnTakeNonZeroDamageGlobal(victim, body);
            }
        }
        }
}