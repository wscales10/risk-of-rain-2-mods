using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Infrastructure;
using R2API;
using RoR2;
using System;
using UnityEngine;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves.Stage1
{
    public class SolusControlUnitMiniBossInfo : PortableMiniBossInfo<SolusControlUnitBossFightBehavior>
    {
        private static readonly Lazy<DeployableSlot> deployableSlot = new Lazy<DeployableSlot>(() => DeployableAPI.RegisterDeployableSlot((_, __) => int.MaxValue));

        private readonly AssetPromise<CharacterSpawnCard> solusControlUnitSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/RoboBallBoss/cscRoboBallBoss.asset");

        private readonly AssetPromise<CharacterSpawnCard> solusProbeSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/RoboBallBoss/cscRoboBallMini.asset");

        public override SpawnInfo SpawnInfo => new SpawnInfo
        {
            count = 1,
            spawnCard = this.solusControlUnitSpawnCard.Value, // TODO: can fail to spawn in Abyssal Depths?
            spawnDistance = Content.MonsterSpawnDistances.WithinZone,
        };

        public override void SetupBossBody(CharacterBody body, SolusControlUnitBossFightBehavior bossFightBehavior)
        {
            if (bossFightBehavior.disableSpecialSkill)
            {
                this.DisableSkill(body, SkillSlot.Special);
            }

            body.EnsureComponent<UndeployMinionsOnDeathBehavior>();
            var spawnMinionsBehavior = body.gameObject.AddComponent<RateLimiter>();
            spawnMinionsBehavior.minimumInterval = 1;
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

                if (Utils.TryGetCharacterBody(probe, out var probeBody))
                {
                    probeBody!.ScaleMaxHealth(this, 1 / 5f);
                    Utils.ScaleDeathRewards(probeBody, 0);
                }

                body.AddMinion(probe, deployableSlot.Value);
                return true;
            };
            SimulacrumWavesModule.OnTakeNonZeroDamageGlobal += (victim, _) => OnTakeNonZeroDamageGlobal(victim, body);
        }

        private static void OnTakeNonZeroDamageGlobal(HealthComponent victim, CharacterBody body)
        {
            if (victim == body.healthComponent)
            {
                body.GetComponent<RateLimiter>().TryDoThing();
            }
        }
    }

    public class SolusControlUnitBossFightBehavior : PortableMiniBossFightBehavior<SolusControlUnitBossFightBehavior>
    {
        public bool disableSpecialSkill = true;
    }

    public sealed class SolusControlUnit : PortableMiniBossWaveDefinition<SolusControlUnitBossFightBehavior>
    {
        public SolusControlUnit() : base(new SolusControlUnitMiniBossInfo())
        {
        }

        protected override PickupDropTable GetRewardDropTable(Run run)
        {
            return BetterExplicitPickupDropTable.ReplaceTierWithSingleItem(GetBaseDropTable(run), RoR2Content.Items.RoboBallBuddy);
        }

        protected override UpgradeEncounterStrategy GetUpgradeStrategy() => ScriptableObject.CreateInstance<EnableSpecialSkillUpgradeStrategy>();

        public class EnableSpecialSkillUpgradeStrategy : UpgradeEncounterStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(EncounterContext ctx)
            {
                var dir = ctx.CombatDirector;

                if (dir.TryGetComponent<SolusControlUnitBossFightBehavior>(out var behavior))
                {
                    behavior.disableSpecialSkill = false;
                }
            }
        }
    }
}