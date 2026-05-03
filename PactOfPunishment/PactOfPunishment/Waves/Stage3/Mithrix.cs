using EntityStates;
using HG;
using PactOfPunishment.AiSkillDrivers;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Infrastructure;
using RoR2;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{

    public class Mithrix : MiniBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        protected override string BaseWavePrefabKey => "RoR2/DLC1/GameModes/InfiniteTowerRun/ITAssets/InfiniteTowerWaveBossBrother.prefab";

        protected override UpgradeEncounterStrategy GetUpgradeStrategy()
        {
            return ScriptableObject.CreateInstance<UpgradeMithrix>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.EnsureComponent<MithrixMiniBossBehavior>().spawnInfo = wavePrefab.spawnList[0];
            wavePrefab.spawnList = Array.Empty<InfiniteTowerExplicitSpawnWaveController.SpawnInfo>();
        }

        public class MithrixMiniBossBehavior : BossFightBehavior, IPreventWaveFromEnding
        {
            public InfiniteTowerExplicitSpawnWaveController.SpawnInfo spawnInfo;

            private BossGroupWrapper? bossGroup;
            
            private InfiniteTowerWaveController? waveController;

            public bool CanWaveEnd { get; private set; }

            public override void Awake()
            {
                base.Awake();
                this.waveController = this.GetComponent<InfiniteTowerWaveController>();
                this.CombatDirector.combatSquad.onDefeatedServer += CombatSquad_onDefeatedServer;
                MoonMusic.Instance.PlayBossTrack(this.waveController);
                this.StartCoroutine(this.SpawnBossAfterDelay());
            }

            private IEnumerator SpawnBossAfterDelay()
            {
                yield return new WaitForSeconds(MoonMusic.BossSpawnDelay);
                this.CombatDirector.Spawn(this.spawnInfo.spawnCard, this.spawnInfo.eliteDef, this.waveController?.spawnTarget.transform, this.spawnInfo.spawnDistance, this.spawnInfo.preventOverhead);
                this.CanWaveEnd = true;
            }

            protected override void OnBossSpawnedServer(CharacterBody body)
            {
                body.ScaleMaxHealth(this, 0.8f);
                Utils.ScaleDeathRewards(body, Utils.CreditsForBossWave(25) / 4000);
                body.EnsureComponent<MithrixBodyBehavior>();
                this.AddBossToGroup(ref this.bossGroup, body);
            }

            private static void CombatSquad_onDefeatedServer()
            {
                if (Run.instance.TryGetComponent<SimulacrumWavesBehavior>(out var behavior))
                {
                    behavior.WasMithrixDefeatedEarlierInRun = true;
                }
            }
        }

        public class MithrixBodyBehavior : BossBodyBehavior
        {
            private FallRiskMitigator fallRiskMitigator;

            private EntityStateMachine bodyStateMachine;

            protected override void Awake()
            {
                base.Awake();
                this.fallRiskMitigator = this.EnsureComponent<FallRiskMitigator>();
                this.fallRiskMitigator.CurrentMode = FallRiskMitigator.Mode.Mithrix;
                this.bodyStateMachine = EntityStateMachine.FindByCustomName(this.gameObject, "Body");
                this.bodyStateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Mage.MageCharacterMain));
                var jetpackStateMachine = this.gameObject.AddComponent<EntityStateMachine>();
                jetpackStateMachine.customName = "Jet";
                jetpackStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
                jetpackStateMachine.mainStateType = new SerializableEntityStateType(typeof(Idle));

                foreach (var ai in this.Body.master.AiComponents.Where(x => x))
                {
                    ai.prioritizePlayers = true;
                    ai.fullVision = true;
                    ai.xrayVision = true;

                    foreach (var skillDriver in ai.GetSkillDrivers("Leap to Center"))
                    {
                        skillDriver.maxUserHealthFraction = 0.5f;
                    }
                }
            }

            protected override void ManagedFixedUpdate(float deltaTime)
            {
                base.ManagedFixedUpdate(deltaTime);

                if (!this.Body)
                {
                    this.fallRiskMitigator.DoUpdate(null);
                    return;
                }

                this.fallRiskMitigator.DoUpdate(this.Body!.transform);

                if (this.fallRiskMitigator.IsAboveGround == false && !(this.bodyStateMachine.state is EntityStates.Mage.FlyUpState))
                {
                    this.bodyStateMachine.SetInterruptState(EntityStateCatalog.InstantiateState(typeof(EntityStates.Mage.FlyUpState)), InterruptPriority.Skill);
                }
            }
        }

        public class UpgradeMithrix : UpgradeEncounterStrategy
        {
            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MainBoss;

            public override void PostInitialise(EncounterContext ctx)
            {
                ctx.CombatDirector.AddSpawnListener(OnBossSpawnedServer);
                ctx.GameObject.EnsureComponent<PhaseCounter>().phase = 3;
                MoonMusic.Instance.SetPhase3();
            }

            private static void OnBossSpawnedServer(GameObject spawnedEntity)
            {
                if (Utils.TryGetCharacterBody(spawnedEntity, out var body) && body.name.Contains("Brother"))
                {
                    body.EnsureComponent<UpgradeMithrixBodyBehavior>();
                }
            }
        }

        public class UpgradeMithrixBodyBehavior : BossBodyBehavior
        {
            protected override void Awake()
            {
                base.Awake();

                this.Body.ScaleMaxHealth(this, 8f / 7);
                this.Body.inventory.GiveItemPermanent(RoR2Content.Items.SprintBonus, 2);
                this.Body.inventory.GiveItemPermanent(RoR2Content.Items.SecondarySkillMagazine, 2);
            }
        }
    }
}