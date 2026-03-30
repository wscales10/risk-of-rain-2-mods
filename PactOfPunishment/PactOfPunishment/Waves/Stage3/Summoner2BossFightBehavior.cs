using HG;
using PactOfPunishment.Waves.Common;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{
    public class Summoner2BossFightBehavior : PortableMiniBossFightBehavior<Summoner2BossFightBehavior>
    {
        public bool disableTeleport = true;

        internal static AssetPromise<CharacterSpawnCard> eggSpawnCard;

        internal static AssetPromise<CharacterSpawnCard> parentSpawnCard;

        private CharacterBody eggBodyPrefab;

        private DoSomethingAtFixedRate? eggSpawner;

        private WeightedSelection<EliteDef> eliteDefs;

        private float eggSpawnerStartTimer;

        private bool? isEggSpawnerEnabled;

        public override void Awake()
        {
            base.Awake();

            this.eliteDefs = this.CombatDirector.GetEliteDefSelector(parentSpawnCard.Value);

            this.eggBodyPrefab = eggSpawnCard.Value.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<CharacterBody>();

            this.eggSpawner = this.gameObject.AddComponent<DoSomethingAtFixedRate>();
            this.eggSpawner.interval = 3;
            this.eggSpawner.doSomething = this.SpawnEgg;

            this.ApplyEnabledState();
        }

        public void Update()
        {
            this.ManagedUpdate(Time.deltaTime);
        }

        protected override void OnCombatSquadMemberDiscovered(CharacterBody body)
        {
            base.OnCombatSquadMemberDiscovered(body);

            if (body.Is(RoR2Content.BodyPrefabs.ParentBody))
            {
                body.EnsureHasItem(RoR2Content.Items.UseAmbientLevel);
                body.ScaleDifficultyAsBoss(158, 158, false, false);
                body.EnsureComponent<ParentBehavior>().onDeathStart = this.OnParentDeath;
            }
            else if (body.Is(this.eggBodyPrefab))
            {
                body.ScaleMaxHealth(this, 1 / 3f);
            }
        }

        private void ManagedUpdate(float deltaTime)
        {
            if (this.CustomEnabled)
            {
                switch (this.isEggSpawnerEnabled)
                {
                    case true:
                        break;

                    case false:
                        this.eggSpawnerStartTimer += deltaTime;

                        if (this.eggSpawnerStartTimer > 3)
                        {
                            this.eggSpawner!.enabled = true;
                            this.isEggSpawnerEnabled = true;
                        }
                        break;

                    default:
                        this.eggSpawnerStartTimer = 0;
                        this.isEggSpawnerEnabled = false;
                        break;
                }
            }
            else
            {
                if (this.isEggSpawnerEnabled != null)
                {
                    this.eggSpawner!.enabled = false;
                    this.isEggSpawnerEnabled = null;
                }
            }
        }

        private void OnParentDeath(ParentBehavior behavior)
        {
            BodySplitter bodySplitter = new BodySplitter
            {
                body = behavior.GetComponent<CharacterBody>(),
                count = 1,
                splinterInitialVelocityLocal = Vector3.zero,
                minSpawnCircleRadius = 0,
                moneyMultiplier = 1
            };
            bodySplitter.masterSummon.masterPrefab = eggSpawnCard.Value.prefab;
            bodySplitter.Perform();
        }

        private void SpawnEgg()
        {
            if (!this.CustomEnabled)
            {
                return;
            }

            var spawnTarget = this.EncounterContext.SpawnTarget;

            if (!spawnTarget)
            {
                return;
            }

            this.CombatDirector.Spawn(eggSpawnCard.Value, this.eliteDefs.Evaluate(this.CombatDirector.rng.nextNormalizedFloat), spawnTarget.transform, DirectorCore.MonsterSpawnDistance.Standard, false);
        }
    }
}