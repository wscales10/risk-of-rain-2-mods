using EntityStates;
using EntityStates.BrotherMonster;
using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Halcyonites;
using PactOfPunishment.Waves.Infrastructure;
using RoR2;
using UnityEngine;
using static RoR2.InfiniteTowerExplicitSpawnWaveController;

namespace PactOfPunishment.Waves.Stage3
{
    public partial class MithrixWithHalcyonite : MainBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private readonly AssetPromise<CharacterSpawnCard> mithrixSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/cscBrotherIT.asset");

        private readonly AssetPromise<CharacterSpawnCard> halcyoniteSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Halcyonite/cscHalcyonite.asset");

        protected override string BaseWavePrefabKey => "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveBossBrother.prefab";

        protected override ItemTier RewardDisplayTier => ItemTier.Tier3;

        protected override PickupDropTable GetRewardDropTable(Run run)
        {
            return BossDropTables.Instance.GetLegendary(run);
        }

        protected override UpgradeEncounterStrategy GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<UpgradeMithrixAndHalcyonite>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.spawnList = new SpawnInfo[]
            {
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = this.mithrixSpawnCard.Value,
                },
                new SpawnInfo
                {
                    count = 1,
                    spawnCard = this.halcyoniteSpawnCard.Value,
                }
            };
            wavePrefab.EnsureComponent<MithrixWithHalcyoniteBehavior>();
        }

        public class MithrixWithHalcyoniteBehavior : BossFightBehavior
        {
            private BossGroupWrapper? mithrixBossGroup;

            private BossGroupWrapper? halcyoniteBossGroup;

            public override void Awake()
            {
                base.Awake();
                this.EnsureComponent<FistsController>();
            }

            protected override void OnBossSpawnedServer(CharacterBody body)
            {
                if (body.name.Contains("Brother"))
                {
                    this.AddBossToGroup(ref this.mithrixBossGroup, body);
                    body.ScaleDifficultyAsBoss(2.5f, 30, true, false);
                    body.EnsureComponent<Mithrix.MithrixBodyBehavior>();

                    // TODO: Mithrix is almost invisible in this fight - fix that
                    if (body.TryGetComponent<CharacterDeathBehavior>(out var component))
                    {
                        component.deathState = new SerializableEntityStateType(typeof(TrueDeathState)); // TODO: ensure his corpse is not sprinting
                    }

                    body.master.onBodyStart += OnMithrixBodyStart;
                }
                else if (body.Is(DLC2Content.BodyPrefabs.HalcyoniteBody))
                {
                    this.AddBossToGroup(ref this.halcyoniteBossGroup, body);
                    var halcyoniteBodyBehavior = body.EnsureComponent<FinalHalcyoniteBodyBehavior>();
                    halcyoniteBodyBehavior.DesiredState = FinalHalcyoniteBodyBehavior.State.Collective;
                    halcyoniteBodyBehavior.BossStateMachine.SetState(new FinalHalcyoniteStates.Phase1());
                }
            }

            private static void OnMithrixBodyStart(CharacterBody body)
            {
                if (Run.instance.TryGetComponent<SimulacrumWavesBehavior>(out var behavior) && behavior.WasMithrixDefeatedEarlierInRun)
                {
                    HealthComponent healthComponent = body.healthComponent;

                    if (healthComponent)
                    {
                        healthComponent.Networkhealth = healthComponent.fullHealth * 0.8f;
                    }
                }
            }
        }

        public class UpgradeMithrixAndHalcyonite : UpgradeEncounterStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MainBoss;

            public override void PostInitialise(EncounterContext ctx)
            {
                ctx.CombatDirector.AddSpawnListener(this.OnBossSpawnedServer);
                ctx.GameObject.EnsureComponent<PhaseCounter>().phase = 3;
            }

            private void OnBossSpawnedServer(GameObject spawnedEntity)
            {
                if (Utils.TryGetCharacterBody(spawnedEntity, out var body) && body.name.Contains("Brother"))
                {
                    body.EnsureComponent<Mithrix.UpgradeMithrixBodyBehavior>();
                }
                else
                {
                    body.EnsureComponent<FinalHalcyoniteBodyBehavior>().DesiredState = FinalHalcyoniteBodyBehavior.State.Gilded;
                    body.ScaleMaxHealth(this, 4f / 3);
                }
            }
        }
    }
}