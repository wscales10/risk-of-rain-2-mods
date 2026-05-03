using EntityStates.Geode;
using HG;
using PactOfPunishment.AiSkillDrivers;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Infrastructure;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.FinalStage
{
    [RequireComponent(typeof(MeridianEventTriggerInteraction))]
    public class FinalBossUpgradeStrategies : MonoBehaviour
    {
        public static float LunarRainDurationMultiplier = 0.3f;

        private string? specialSkillDriver;

        public UpgradeEncounterStrategy? GetUpgradeStrategy(FalseSonBossFightContext ctx)
        {
            BaseStrategy? strategy;

            switch (ctx.PhaseState)
            {
                case EntityStates.MeridianEvent.Phase1 _:
                    strategy = ScriptableObject.CreateInstance<Phase1>();
                    break;

                case EntityStates.MeridianEvent.Phase2 _:
                    strategy = ScriptableObject.CreateInstance<Phase2>();
                    break;

                case EntityStates.MeridianEvent.Phase3 _:
                    strategy = ScriptableObject.CreateInstance<Phase3>();
                    break;

                default:
                    return null;
            }

            strategy.Controller = this;
            return strategy;
        }

        public abstract class BaseStrategy : UpgradeEncounterStrategy
        {
            public FinalBossUpgradeStrategies Controller;

            public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MainBoss;

            public override void PostInitialise(EncounterContext ctx)
            {
                ctx.CombatSquad.onMemberAddedServer += this.CombatSquad_onMemberAddedServer;
            }

            protected abstract void OnBossSpawnedServer(CharacterBody body, BaseAI ai);

            protected void EnablePathfindingRoundGeode(CharacterBody body)
            {
                body.useTemporaryPathfindingFootposition = true;
                body.EnsureComponent<ObstacleNavigator>();
            }

            private void CombatSquad_onMemberAddedServer(CharacterMaster characterMaster)
            {
                var body = characterMaster.GetBody();

                if (FinalBoss.IsFalseSonBoss(body))
                {
                    var ai = characterMaster.GetComponent<BaseAI>();
                    this.EnablePathfindingRoundGeode(body);

                    foreach (var skillDriver in ai.GetSkillDrivers("Lunar Rain"))
                    {
                        skillDriver.driverUpdateTimerOverride *= FinalBossUpgradeStrategies.LunarRainDurationMultiplier;
                    }

                    this.OnBossSpawnedServer(body, ai);
                }
            }
        }

        public class Phase1 : BaseStrategy
        {
            protected override void OnBossSpawnedServer(CharacterBody body, BaseAI ai)
            {
                ai.xrayVision = true;
                this.Controller.specialSkillDriver ??= JsonUtility.ToJson(ai.skillDrivers.First(x => x.skillSlot == SkillSlot.Special));
                body.EnsureComponent<Phase1BodyBehavior>();
            }
        }

        public class Phase2 : BaseStrategy, UpgradedLeap.IUpgradedLeapController
        {
            private static bool isTestMode = true;

            private readonly AssetPromise<CharacterSpawnCard> falseSonBossSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/FalseSonBoss/cscFalseSonBoss.asset");

            private readonly IPortableMiniBossWaveDefinition[] portableMiniBossWaveDefinitions = SimulacrumWavesModule.Instance.Cache.GetPortableMiniBossWaveDefinitions();

            private GameObject miniBossCombatDirectorGameObject;

            private bool hasDoneUpgradedLeap = false;

            public override void PreInitialise(EncounterContext ctx)
            {
                base.PreInitialise(ctx);
                this.ModifyPhase2CombatDirector(MeridianEventTriggerInteraction.instance.phase2CombatDirector.GetComponent<CombatDirector>());
                this.miniBossCombatDirectorGameObject.EnsureComponent<EncounterContextHolder>().encounterContext = new FalseSonMinionContext(MeridianEventTriggerInteraction.instance);
            }

            public void TryUpgradedLeap(UpgradedLeap.UpgradedLeapBodyBehavior sender)
            {
                if (this.hasDoneUpgradedLeap && !isTestMode)
                {
                    return;
                }

                MeridianEventTriggerInteraction.instance.StartCoroutine(this.TryUpgradedLeapInternal(sender));
            }

            protected override void OnBossSpawnedServer(CharacterBody body, BaseAI ai)
            {
                ai.RemoveSkillDriversWhere(x => x.skillSlot == SkillSlot.Special);
                var newSkillDriver = ai.gameObject.AddComponent<AISkillDriver>();
                JsonUtility.FromJsonOverwrite(this.Controller.specialSkillDriver, newSkillDriver);
                ai.InsertSkillDriver(newSkillDriver, Array.FindIndex(ai.skillDrivers, x => x.skillSlot == SkillSlot.Utility) + 1);
                body.EnsureComponent<Phase2BodyBehavior>();
                body.EnsureComponent<UpgradedLeap.UpgradedLeapBodyBehavior>().Controller = this;
            }

            private void ModifyPhase2CombatDirector(CombatDirector combatDirector)
            {
                // TODO: weights
                combatDirector.monsterCards = Utils.MakeDirectorCardCategorySelection(("Minibosses", this.portableMiniBossWaveDefinitions.SelectMany(x => x.MiniBosses).Select<IPortableMiniBossInfo, SpawnCard>(x => x.SpawnInfo.spawnCard).ToArray()));
                combatDirector.moneyWaveIntervals = Array.Empty<RangeFloat>();
                combatDirector.moneyWaves = Array.Empty<CombatDirector.DirectorMoneyWave>();
                combatDirector.monsterCredit = 0;
                combatDirector.maxSquadCount = 0;
                combatDirector.EnsureComponent<MiniBossSpawner>();
                this.miniBossCombatDirectorGameObject = combatDirector.gameObject;

                foreach (var portableMiniBossWaveDefinition in this.portableMiniBossWaveDefinitions)
                {
                    portableMiniBossWaveDefinition.EnsureGameObjectHasBossFightBehavior(this.miniBossCombatDirectorGameObject); // TODO: disable?
                }
            }

            private IEnumerator TryUpgradedLeapInternal(UpgradedLeap.UpgradedLeapBodyBehavior sender)
            {
                yield return new WaitForSeconds(1);

                if (sender.Body.master.TryGetComponent<BaseAI>(out var ai))
                {
                    if (ai.currentEnemy.characterBody)
                    {
                        this.hasDoneUpgradedLeap = true;

                        var spawnedInstance = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(this.falseSonBossSpawnCard.Value, new DirectorPlacementRule
                        {
                            placementMode = DirectorPlacementRule.PlacementMode.Direct,
                            preventOverhead = false,
                            rotation = Quaternion.identity,
                            spawnOnTarget = sender.Body.transform
                        }, MeridianEventTriggerInteraction.instance.falseSonBossGroup.rng)
                        {
                            teamIndexOverride = sender.Body.teamComponent.teamIndex, // Do not set summonerBodyObject as we don't want the ghosts in the combat squad
                            ignoreTeamMemberLimit = true,
                        });

                        if (Utils.TryGetCharacterBody(spawnedInstance, out var spawnedBody))
                        {
                            var upgradedLeapGhostBehavior = spawnedBody.EnsureComponent<UpgradedLeap.UpgradedLeapGhostBehavior>();
                            upgradedLeapGhostBehavior.TargetBody = ai.currentEnemy.characterBody;
                        }
                    }
                }
            }
        }

        public class Phase3 : BaseStrategy
        {
            public override void PostInitialise(EncounterContext ctx)
            {
                base.PostInitialise(ctx);
                MeridianEventTriggerInteraction.instance.colossusHead.EnsureComponent<FireLaserMore.FireLaserMoreBehavior>();
                MeridianEventTriggerInteraction.instance.EnsureComponent<RepositionGeodesBehavior>().Centre = MeridianEventTriggerInteraction.instance.arenaCenter;
            }

            protected override void OnBossSpawnedServer(CharacterBody body, BaseAI ai)
            {
                body.EnsureComponent<Phase3BodyBehavior>();
            }
        }

        public abstract class BodyBehavior : BossBodyBehavior
        {
            public void OnEnable()
            {
                RecalculateStats.Add(this.Body, this.OnRecalculateStats);
            }

            public void OnDisable()
            {
                RecalculateStats.Remove(this.Body, this.OnRecalculateStats);
            }

            internal void OnSpawnLunarRaindrop(GameObject spawnedInstance)
            {
                spawnedInstance.EnsureComponent<MultiplyMaxHealthBehavior>().Multipliers[this] = 0.25f;
            }

            protected virtual void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                // Do nothing by default
            }
        }

        public class Phase1BodyBehavior : BodyBehavior
        {
            private FinalBoss.PredictiveDashController predictiveDashController;

            protected override void Awake()
            {
                base.Awake();
                this.Body.skillLocator.utility.skillDef.dontAllowPastMaxStocks = false;
                this.Body.skillLocator.utility.OverrideRechargeStock((orig, self) =>
                {
                    if ((this.predictiveDashController.DidMostRecentDashUsePredictedPosition) || (self.characterBody.TryGetComponent<ObstacleNavigator>(out var obstacleNavigator) && obstacleNavigator.IsInNavigateAroundObstacleMode))
                    {
                        return orig;
                    }
                    else
                    {
                        return UnityEngine.Random.RandomRangeInt(orig, self.maxStock);
                    }
                });
                this.predictiveDashController = this.EnsureComponent<FinalBoss.PredictiveDashController>();
                this.predictiveDashController.GetLookAheadDuration = () => FinalBoss.GetPreFissureSlamDuration(this.Body) + 1.1f;
                this.predictiveDashController.ShouldUsePredictedPosition = () => UnityEngine.Random.value < 1 / 3f;
            }

            protected override void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                base.OnRecalculateStats(args);
                args.utilitySkill.bonusStockAdd++;
                args.healthTotalMult *= 1.4f;
            }
        }

        public class Phase2BodyBehavior : BodyBehavior
        {
            protected override void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                base.OnRecalculateStats(args);
                args.healthTotalMult *= 2;
            }
        }

        public class Phase3BodyBehavior : BodyBehavior
        {
            protected override void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                base.OnRecalculateStats(args);
                args.healthTotalMult *= 1.1f;
            }
        }

        public class MiniBossSpawner : MonoBehaviour
        {
            private int spawned = 0;

            private EntityStates.MeridianEvent.Phase2 phase2;

            private CombatDirector combatDirector;

            public void Awake()
            {
                this.phase2 = (EntityStates.MeridianEvent.Phase2)MeridianEventTriggerInteraction.instance.mainStateMachine.state;
                this.combatDirector = MeridianEventTriggerInteraction.instance.phase2CombatDirector.GetComponent<CombatDirector>();
            }

            public void Update()
            {
                if (this.spawned < this.HowManyMiniBossesShouldIHaveSpawned())
                {
                    this.combatDirector.ignoreCostOfNextMonster = true;

                    if (this.combatDirector.AttemptSpawnOnTarget(MeridianEventTriggerInteraction.instance.arenaCenter))
                    {
                        this.spawned++;
                    }
                }
            }

            private int HowManyMiniBossesShouldIHaveSpawned()
            {
                if (!this.phase2.hasSpawned)
                {
                    return 0;
                }

                float healthFraction = this.phase2.phaseBossGroup.totalObservedHealth / Mathf.Max(this.phase2.phaseBossGroup.totalMaxObservedMaxHealth, 1f);

                if (healthFraction < 0.4f)
                {
                    return 3;
                }
                else
                {
                    return 1;
                }
            }
        }

        public class RepositionGeodesBehavior : MonoBehaviour
        {
            private readonly List<NodeGraph.NodeIndex> nodes = new List<NodeGraph.NodeIndex>();

            private Transform? centre;

            public Transform? Centre
            {
                get => this.centre;
                set
                {
                    this.centre = value;
                    this.UpdateNodes();
                }
            }

            public void OnEnable()
            {
                this.UpdateNodes();
            }

            internal void GeodeBecameInert(GeodeInert self)
            {
                if (this.nodes.Count > 0)
                {
                    var nodeIndex = Run.instance.stageRng.NextElementUniform(this.nodes);

                    if (SceneInfo.instance.groundNodes.GetNodePosition(nodeIndex, out var nodePosition))
                    {
                        self.transform.position = nodePosition;
                        return;
                    }
                }

                Debug.LogWarning("Unable to get node position for geode.");
            }

            private void UpdateNodes()
            {
                this.nodes.Clear();

                if (this.Centre)
                {
                    this.nodes.AddRange(SceneInfo.instance.groundNodes.FindNodesInRange(this.Centre!.position, 15, 70, HullMask.Golem));
                }
            }
        }
    }
}