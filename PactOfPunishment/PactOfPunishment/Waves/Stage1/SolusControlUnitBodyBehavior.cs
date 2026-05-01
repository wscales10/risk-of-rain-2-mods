using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1
{
    public partial class SolusControlUnitMiniBossInfo
    {
        public class SolusControlUnitBodyBehavior : BossBodyBehavior, IOnTakeDamageServerReceiver
        {
            private static float minimumAltitude = 13f;

            private readonly AssetPromise<CharacterSpawnCard> solusProbeSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/RoboBallBoss/cscRoboBallMini.asset");

            private Rigidbody rigidbody;

            private RateLimiter spawnMinionsBehavior;

            public void OnEnable()
            {
                RecalculateStats.Add(this.Body, OnRecalculateStats);
            }

            public void OnDisable()
            {
                RecalculateStats.Remove(this.Body, OnRecalculateStats);
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (damageReport.damageDealt > 0)
                {
                    this.spawnMinionsBehavior.TryDoThing();
                }
            }

            internal bool TrySetupProbe(GameObject probe)
            {
                if (!Utils.TryGetCharacterBody(probe, out var probeBody))
                {
                    Debug.LogError("Failed to get probe body after spawning it");
                    return false;
                }

                probeBody.ScaleMaxHealth(this, 0.2f);
                probeBody.ScaleDamage(this, 2 / 3f);

                Inventory probeInventory = probe.GetComponent<Inventory>();
                probeInventory.SetEquipmentIndex(this.Body.inventory.currentEquipmentIndex, false);

                if (this.Body.inventory.GetItemCountEffective(RoR2Content.Items.Ghost) > 0)
                {
                    probeInventory.GiveItemPermanent(RoR2Content.Items.Ghost);
                    probeInventory.GiveItemPermanent(RoR2Content.Items.HealthDecay, 30);
                    probeInventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, 150);
                }

                Utils.ScaleDeathRewards(probeBody, 0);
                this.Body.AddMinion(probe, deployableSlot.Value);
                return true;
            }

            protected override void ManagedFixedUpdate(float deltaTime)
            {
                base.ManagedFixedUpdate(deltaTime);

                if (Physics.Raycast(this.Body.corePosition, Vector3.down, minimumAltitude, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    var velocity = this.rigidbody.velocity;

                    if (velocity.y < 5)
                    {
                        velocity.y += this.Body.acceleration * deltaTime;
                    }

                    this.rigidbody.velocity = velocity;
                }
            }

            protected override void Awake()
            {
                base.Awake();
                this.rigidbody = this.GetComponent<Rigidbody>();

                this.spawnMinionsBehavior = this.gameObject.AddComponent<RateLimiter>();
                this.spawnMinionsBehavior.minimumInterval = 1;
                this.spawnMinionsBehavior.doThing = this.TrySpawnProbe;

                this.Body.healthComponent.AddOnTakeDamageServerReceiver(this);
            }

            private static void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedTotalMult *= 2;
                args.utilitySkill.cooldownMultiplier /= 3f;
            }

            private bool TrySpawnProbe()
            {
                DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Close, out var minDistance, out var maxDistance);
                var directorSpawnRequest = new DirectorSpawnRequest(this.solusProbeSpawnCard.Value, new DirectorPlacementRule
                {
                    minDistance = minDistance,
                    maxDistance = maxDistance,
                    position = this.Body.corePosition,
                }, RoR2Application.rng)
                {
                    summonerBodyObject = this.gameObject
                };

                var probe = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                return this.TrySetupProbe(probe);
            }
        }
    }
}