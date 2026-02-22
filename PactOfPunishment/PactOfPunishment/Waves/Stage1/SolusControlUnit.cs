using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Infrastructure;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves.Stage1
{
    public sealed class SolusControlUnit : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private static readonly Lazy<DeployableSlot> deployableSlot = new Lazy<DeployableSlot>(() => DeployableAPI.RegisterDeployableSlot((_, __) => int.MaxValue));

        private readonly AssetPromise<CharacterSpawnCard> solusControlUnitSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/RoboBallBoss/cscRoboBallBoss.asset");

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = this.solusControlUnitSpawnCard.Value,
                }
            };

            dir.gameObject.AddComponent<SolusControlUnitBossFightBehavior>();
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

        protected override UpgradeWaveStrategy GetUpgradeStrategy() => ScriptableObject.CreateInstance<EnableSpecialSkillUpgradeStrategy>();

        public class SolusControlUnitBossFightBehavior : BossFightBehavior
        {
            public bool disableSpecialSkill = true;

            private readonly AssetPromise<CharacterSpawnCard> solusProbeSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/RoboBallBoss/cscRoboBallMini.asset");

            private static void OnTakeNonZeroDamageGlobal(HealthComponent victim, CharacterBody body)
            {
                if (victim == body.healthComponent)
                {
                    victim.GetComponent<RateLimiter>().TryDoThing();
                }
            }

            protected override void OnBossSpawnedServer(CharacterBody body)
            {
                if (!body.Is(RoR2Content.BodyPrefabs.RoboBallBossBody))
                {
                    return;
                }

                if (this.disableSpecialSkill)
                {
                    Utils.DisableSkill(body, x => x.special);
                }

                body.EnsureComponent<UndeployMinionsOnDeathBehavior>();
                var spawnMinionsBehavior = body.gameObject.AddComponent<RateLimiter>();
                spawnMinionsBehavior.doThing = () =>
                {
                    DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Close, out var minDistance, out var maxDistance);
                    var directorSpawnRequest = new DirectorSpawnRequest(this.solusProbeSpawnCard.Value, new DirectorPlacementRule
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

                    Inventory probeInventory = probe.GetComponent<Inventory>();
                    probeInventory.SetEquipmentIndex(body.inventory.currentEquipmentIndex, false);

                    if (body.inventory.GetItemCountEffective(RoR2Content.Items.Ghost) > 0)
                    {
                        probeInventory.GiveItemPermanent(RoR2Content.Items.Ghost);
                        probeInventory.GiveItemPermanent(RoR2Content.Items.HealthDecay, 30);
                        probeInventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, 150);
                    }

                    CharacterBody? probeBody = Utils.GetCharacterBody(probe);

                    if (probeBody)
                    {
                        probeBody!.ScaleMaxHealth(this, 1 / 5f);
                        Utils.ScaleDeathRewards(probeBody, 0);
                    }

                    body.AddMinion(probe, deployableSlot.Value);
                    return true;
                };
                SimulacrumWavesModule.OnTakeNonZeroDamageGlobal += (victim, _) => OnTakeNonZeroDamageGlobal(victim, body);
            }
        }

        public class EnableSpecialSkillUpgradeStrategy : UpgradeWaveStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(InfiniteTowerWaveController wave)
            {
                var dir = wave.combatDirector;

                if (dir.TryGetComponent<SolusControlUnitBossFightBehavior>(out var behavior))
                {
                    behavior.disableSpecialSkill = false;
                }
            }
        }
    }
}