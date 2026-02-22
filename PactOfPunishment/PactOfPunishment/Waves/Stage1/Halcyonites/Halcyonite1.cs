using EntityStates.Halcyonite;
using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    public partial class Halcyonite1 : MainBossWaveDefinition<InfiniteTowerExplicitSpawnWaveController>
    {
        private readonly AssetPromise<CharacterSpawnCard> halcyoniteSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Halcyonite/cscHalcyonite.asset");

        private readonly AssetPromise<CharacterSpawnCard>[] meleeSpawnCards = new string[] { "", "Nature", "Sandy", "Snowy" }.Select(env => Utils.BeginLoad<CharacterSpawnCard>($"RoR2/Base/Golem/cscGolem{env}.asset")).ToArray();

        private readonly AssetPromise<CharacterSpawnCard> rangedSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Child/cscChild.asset");

        public static void SetLaserBehaviorEnabled(BaseAI ai, bool shouldBeEnabled) // TODO: enable/disable skill instead, and make behavior dependent on skill?
        {
            foreach (var skillDriver in ai.GetSkillDrivers("TriLaser"))
            {
                skillDriver.enabled = shouldBeEnabled;
            }
        }

        protected override UpgradeWaveStrategy? GetUpgradeStrategy()
        {
            return null; // TODO: Extreme measures 1
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);

            wavePrefab.spawnList = new InfiniteTowerExplicitSpawnWaveController.SpawnInfo[]
            {
                new InfiniteTowerExplicitSpawnWaveController.SpawnInfo
                {
                    count = 1,
                    spawnCard = this.halcyoniteSpawnCard.Value
                }
            };
            wavePrefab.EnsureComponent<HalcyoniteBossFightBehavior>();
            wavePrefab.EnsureComponent<KeepCombatDirectorEnabledBehavior>();

            var monsterCards = ScriptableObject.CreateInstance<DirectorCardCategorySelection>();
            monsterCards.AddCategory("Ranged", 1);

            monsterCards.AddCard(0, new DirectorCard
            {
                selectionWeight = 1,
                spawnCard = this.rangedSpawnCard.Value,
            });

            monsterCards.AddCategory("Melee", 1);

            foreach (var promise in this.meleeSpawnCards)
            {
                monsterCards.AddCard(1, new DirectorCard
                {
                    selectionWeight = 1,
                    spawnCard = promise.Value,
                });
            }

            dir._monsterCards = monsterCards;
            dir.maxSquadCount = 4;
            dir.minRerollSpawnInterval = 0.5f;
            dir.maxRerollSpawnInterval = 0.5f;
            dir.minSeriesSpawnInterval = 4f;
            dir.maxSeriesSpawnInterval = 6f;
            dir.moneyWaveIntervals = new RangeFloat[]
            {
                new RangeFloat
                {
                    min = 1,
                    max = 1
                }
            };
            dir.creditMultiplier = 3.6f;
            dir.EnsureComponent<DisableWhileSquadFullBehavior>();
        }

        public class HalcyoniteBossFightBehavior : BossFightBehavior
        {
            private bool? laserFirst;

            public override void Awake()
            {
                base.Awake();
                string sceneName = SceneCatalog.GetSceneDefForCurrentScene().baseSceneName;

                this.CombatDirector.monsterCards.RemoveCardsThatFailFilter(x =>
                {
                    string cardName = x.spawnCard.name;
                    return !cardName.Contains("cscGolem") || FilterGolemCard(cardName, sceneName);
                });

                this.CombatDirector.monsterCardsSelection = this.CombatDirector.monsterCards.GenerateDirectorCardWeightedSelection();

                this.EliminateCombatSquadWhenLastMainMemberDies(this.CombatDirector.combatSquad, x => x.GetBody()?.bodyIndex == DLC2Content.BodyPrefabs.HalcyoniteBody.bodyIndex, () => this.CombatDirector.enabled = false);
            }

            private static bool FilterGolemCard(string cardName, string sceneName)
            {
                switch (sceneName)
                {
                    case "itancientloft": // Aphelian Sanctuary
                    case "itgoolake": // Abandoned Aqueduct
                        return cardName.Contains("Sandy");

                    case "itfrozenwall": // Rallypoint Delta
                        return cardName.Contains("Snowy");

                    case "itgolemplains":
                        return cardName.Contains("Nature");

                    default:
                        return cardName == "cscGolem";
                }
            }

            protected override void OnBossSpawnedServer(CharacterBody body)
            {
                if (body.Is(DLC2Content.BodyPrefabs.HalcyoniteBody))
                {
                    body.ScaleDifficultyAsBoss(0.65f, 65f, true, false); // TODO: rethink the way I'm scaling enemies, I need one or more helper methods which easily allow me to correctly scale enemy health, damage and most importantly, rewards. Also note that the combat squads scale enemy health for multiplayer by default, so at the moment I'm overscaling.
                    this.laserFirst ??= this.GetComponent<CombatDirector>().rng.nextBool;

                    SetupBossAi(body);

                    var halcyoniteBodyBehavior = body.EnsureComponent<Halcyonite1BodyBehavior>();
                    halcyoniteBodyBehavior.laserFirst = this.laserFirst.Value;
                    body.inventory.GiveItemPermanent(RoR2Content.Items.SecondarySkillMagazine, 2);
                    body.DisableStunsEtc();
                }
                else if (body.Is(RoR2Content.BodyPrefabs.GolemBody))
                {
                    body.ScaleDifficultyAsBoss(2.5f, 30f, true, false);
                    Utils.DisableSkill(body, x => x.secondary);
                }
                else if (body.Is(DLC2Content.BodyPrefabs.ChildBody))
                {
                    body.ScaleDifficultyAsBoss(2.5f, 30f, true, false);
                    body.EnsureComponent<DisableChildMonsterTeleport>();
                }
            }

            private static void SetupBossAi(CharacterBody body)
            {
                foreach (var ai in body.master.AiComponents)
                {
                    ai.prioritizePlayers = true;
                    ai.aimVectorMaxSpeed = 720f; // Turn twice as fast

                    foreach (var skillDriver in ai.GetSkillDrivers("Golden Swipe"))
                    {
                        // Increase max activation distance of thrust, as it will move the
                        // Halcyonite forward
                        skillDriver.maxDistance += 16;
                    }

                    int index = Array.FindIndex(ai.skillDrivers, x => x.customName == "WhirlwindRush");

                    if (index != -1)
                    {
                        var whirlwindSkillDriver = ai.skillDrivers[index];

                        // Increase min activation distance of whirlwind, so the Halcyonite uses
                        // thrust instead more often
                        whirlwindSkillDriver.minDistance += 10;

                        // Disable this behavior if new skill is active
                        whirlwindSkillDriver.requiredSkill = HalcyoniteModule.WhirlwindSkillDef; // TODO: check this, maybe loading asset is not the correct way.

                        var skillDrivers = ai.skillDrivers;
                        var newSkillDriver = ai.gameObject.AddComponent<AISkillDriver>();
                        HalcyoniteModule.UntitledSkillState.SetupSkillDriver(newSkillDriver);
                        ArrayUtils.ArrayInsert(ref skillDrivers, index + 1, newSkillDriver);
                        ai.ReplaceSkillDrivers(skillDrivers);
                    }

                    SetLaserBehaviorEnabled(ai, false);
                }
            }
        }

        public class Halcyonite1BodyBehavior : HalcyoniteBodyBehavior // TODO: do these classes this live on both client and server, or just server? Should there be checks for NetworkServer.active? Is using properties instead of fields okay? Should it be a NetworkBehavior? Same question applies to many of my behaviors.
        {
            public bool laserFirst;

            private readonly List<SwipeTimer> swipeTimers = new List<SwipeTimer>();

            public static void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.attackSpeedTotalMult *= 0.5f;
                args.primarySkill.cooldownMultiplier *= 0.5f;
                args.specialSkill.cooldownMultiplier *= 4 / 3f;
            }

            public override void Awake()
            {
                base.Awake();
                var stateMachine = this.gameObject.AddComponent<EntityStateMachine>();
                stateMachine.customName = "BossBody";
                this.Body?.healthComponent.ForwardBossDamageTo(stateMachine);
                stateMachine.SetState(new Halcyonite1States.Phase1());
            }

            public void OnEnable()
            {
                RecalculateStats.Add(this.GetComponent<CharacterBody>(), OnRecalculateStats);
            }

            public void OnDisable()
            {
                RecalculateStats.Remove(this.GetComponent<CharacterBody>(), OnRecalculateStats);
            }

            public void OnSwipe(GoldenSwipe state)
            {
                if (state.isAuthority && state.characterBody && state.characterBody.characterMotor)
                {
                    this.swipeTimers.Add(new SwipeTimer { Timer = state.duration * 0.35f, State = state });
                }
            }

            protected override void FixedUpdate(float deltaTime)
            {
                base.FixedUpdate(deltaTime);

                for (int i = this.swipeTimers.Count - 1; i >= 0; i--)
                {
                    this.UpdateSwipeTimer(i, Time.fixedDeltaTime);
                }
            }

            private void UpdateSwipeTimer(int index, float deltaTime)
            {
                var state = this.swipeTimers[index].State;

                if (state.outer?.state != state)
                {
                    this.swipeTimers.RemoveAt(index);
                    return;
                }

                this.swipeTimers[index].Timer -= deltaTime;

                if (this.swipeTimers[index].Timer >= 0)
                {
                    return;
                }

                this.swipeTimers.RemoveAt(index);
                float mass = this.Body!.characterMotor ? this.Body.characterMotor.mass : 1f;
                float acceleration = this.Body.acceleration;
                float xSpeed = Trajectory.CalculateInitialYSpeedForHeight(16, -acceleration);
                this.Body.characterMotor.ApplyForce(xSpeed * mass * (this.Body.inputBank ? this.Body.inputBank.aimDirection : this.Body.transform.forward));
            }

            private sealed class SwipeTimer
            {
                public float Timer;

                public GoldenSwipe State;
            }
        }
    }
}