using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Stage1.Halcyonites;
using R2API;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{
    public class Summoner2 : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private readonly AssetPromise<CharacterSpawnCard> childSpawnCard;

        public Summoner2()
        {
            this.childSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Child/cscChild.asset");
        }

        protected override UpgradeWaveStrategy? GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<EnableChildTeleportStrategy>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);

            dir.maxSquadCount = 6; // TODO: make max squad count calculation more intelligent

            wavePrefab.spawnList = new InfiniteTowerExplicitSpawnWaveController.SpawnInfo[]
            {
                new InfiniteTowerExplicitSpawnWaveController.SpawnInfo
                {
                    count = 1,
                    spawnCard = this.childSpawnCard.Value,
                }
            };

            wavePrefab.EnsureComponent<Summoner2BossFightBehavior>();
        }

        public class Summoner2BossBodyBehavior : MonoBehaviour
        {
            public void OnEnable()
            {
                RecalculateStats.Add(this.GetComponent<CharacterBody>(), OnRecalculateStats);
            }

            public void OnDisable()
            {
                RecalculateStats.Remove(this.GetComponent<CharacterBody>(), OnRecalculateStats);
            }

            private static void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedTotalMult = 0;
            }
        }

        public class Summoner2BossFightBehavior : BossFightBehavior
        {
            public bool disableTeleport = true;

            internal static AssetPromise<CharacterSpawnCard> eggSpawnCard;

            internal static AssetPromise<CharacterSpawnCard> parentSpawnCard;

            private DoSomethingAtFixedRate? eggSpawner;

            private InfiniteTowerWaveController waveController;

            private EliteDef[] eliteDefs;

            public override void Awake()
            {
                base.Awake();
                this.waveController = this.GetComponent<InfiniteTowerWaveController>();

                this.eliteDefs = Utils.GetEliteDefs(parentSpawnCard.Value).ToArray(); // TODO: this should be more sophisticated

                this.eggSpawner = this.gameObject.AddComponent<DoSomethingAtFixedRate>();
                this.eggSpawner.interval = 3;
                this.eggSpawner.doSomething = this.SpawnEgg;

                Utils.DoSomethingWhenLastMainSquadMemberDies(this.CombatDirector.combatSquad, x => x.GetBody().Is(DLC2Content.BodyPrefabs.ChildBody), this.OnLastMainBossDefeated);
            }

            protected override void OnCombatSquadMemberDiscovered(CharacterBody body)
            {
                base.OnCombatSquadMemberDiscovered(body);

                if (body.Is(RoR2Content.BodyPrefabs.ParentBody))
                {
                    body.ScaleDifficultyAsBoss(158, 158, false, false);
                    Utils.MakeScaledElite(body.inventory ??= body.master.inventory, this.CombatDirector.rng.NextElementUniform(this.eliteDefs));
                }
            }

            protected override void OnBossSpawnedServer(CharacterBody body)
            {
                if (body.Is(DLC2Content.BodyPrefabs.ChildBody))
                {
                    body.EnsureComponent<Summoner2BossBodyBehavior>();
                    body.ScaleDifficultyAsBoss(3, 30, true, false);
                    body.ResistNonTargetedDamage();

                    if (this.disableTeleport)
                    {
                        body.EnsureComponent<DisableChildMonsterTeleport>();
                    }
                }
            }

            private void OnLastMainBossDefeated(CharacterMaster master, DamageReport report)
            {
                this.eggSpawner!.enabled = false;
            }

            private void SpawnEgg()
            {
                var spawnTarget = this.waveController.spawnTarget;

                if (!spawnTarget)
                {
                    return;
                }

                this.CombatDirector.Spawn(eggSpawnCard.Value, null, spawnTarget.transform, DirectorCore.MonsterSpawnDistance.Standard, false);
            }
        }

        public class EnableChildTeleportStrategy : UpgradeWaveStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MiniBoss;

            public override void PostInitialise(InfiniteTowerWaveController wave)
            {
                wave.GetComponent<Summoner2BossFightBehavior>().disableTeleport = false;
            }
        }
    }
}