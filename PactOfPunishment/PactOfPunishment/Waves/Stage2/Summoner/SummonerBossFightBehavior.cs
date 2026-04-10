using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage2.Summoner
{
    public class SummonerBossFightBehavior : MonoBehaviour // TODO: should inherit from BossFightBehavior?
    {
        public const string StateMachineCustomName = "SummonerBossBody";

        public SummonerReferences References;

        private readonly AssetPromise<CharacterSpawnCard> slammerSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/Parent/cscParent.asset");

        private readonly AssetPromise<CharacterSpawnCard> solusProspectorSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC3/WorkerUnit/cscWorkerUnit.asset");

        private CombatDirector? combatDirector;

        public void Awake()
        {
            this.combatDirector = this.GetComponent<CombatDirector>();
            this.combatDirector.EnsureComponent<UseMinimumEliteTierBehavior>();
            MonsterTracker.TrackCombatDirector(this.combatDirector);

            this.References = this.EnsureComponent<SummonerReferences>();
            this.References.MainBossMonsterIndex = this.GetMainBossMonsterIndex();
            this.References.SupportMonsterDirectorCards = this.GetSupportMonsterDirectorCards(2);

            var mainBossMonsterDirectorCard = this.combatDirector.finalMonsterCardsSelection.GetChoice(this.References.MainBossMonsterIndex).value;
            Debug.Log($"Selected summoner boss: '{mainBossMonsterDirectorCard?.spawnCard.prefab.name}'");
            this.combatDirector.OverrideNextBossCard(mainBossMonsterDirectorCard, false); // TODO: can fail! Try on commencement? also note that this implicitly calls ScaleDifficultyAsBoss.
            this.gameObject.EliminateCombatSquadWhenLastMainMemberDies(this.combatDirector.combatSquad, x => EntityStateMachine.FindByCustomName(x.GetBodyObject(), StateMachineCustomName));
        }

        public void OnEnable()
        {
            this.combatDirector.EnsureComponent<InfiniteTowerWaveSpawnListener>().OnSpawnedServer += this.SummonerBossFightBehavior_OnSpawnedServer;
        }

        public void OnDisable()
        {
            this.combatDirector.GetComponent<InfiniteTowerWaveSpawnListener>().OnSpawnedServer -= this.SummonerBossFightBehavior_OnSpawnedServer;
        }

        internal void SpawnGhosts(CharacterBody bossBody, SummonerBossPowerLevel powerLevel)
        {
            bossBody.EnsureComponent<UndeployMinionsOnDeathBehavior>();
            DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Close, out var minimumDistance, out var maximumDistance);
            bossBody.master.onBodyDeath.AddListener(() =>
            {
                var ghostBodies = bossBody.GetComponent<SummonerBossBodyBehavior>().ghostBodies;

                for (int i = ghostBodies.Count - 1; i >= 0; i--)
                {
                    var ghostBody = ghostBodies[i];

                    if (ghostBody)
                    {
                        ghostBody.master?.TrueKill(this.gameObject, this.gameObject, DamageType.VoidDeath);
                    }

                    ghostBodies.RemoveAt(i);
                }
            });

            if (powerLevel != SummonerBossPowerLevel.Support)
            {
                SpawnGhost(this.slammerSpawnCard.Value, SummonerBossType.SlammerGhost);
            }

            SpawnGhost(this.solusProspectorSpawnCard.Value, SummonerBossType.LungerGhost);

            void SpawnGhost(CharacterSpawnCard spawnCard, SummonerBossType bossType)
            {
                var spawnedInstance = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
                {
                    minDistance = minimumDistance,
                    maxDistance = maximumDistance,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    preventOverhead = false,
                    rotation = Quaternion.identity,
                    spawnOnTarget = bossBody.transform
                }, this.combatDirector!.rng)
                {
                    teamIndexOverride = bossBody.teamComponent.teamIndex, // Do not set summonerBodyObject as we don't want the ghosts in the combat squad
                    ignoreTeamMemberLimit = true,
                });

                if (Utils.TryGetCharacterBody(spawnedInstance, out var spawnedBody))
                {
                    spawnedBody.MakeGhost();
                    var ghostBossBodyBehavior = spawnedBody.EnsureComponent<SummonerBossBodyBehavior>();
                    ghostBossBodyBehavior.PowerLevel = powerLevel;
                    ghostBossBodyBehavior.BossType = bossType;
                    ghostBossBodyBehavior.BodyCost = spawnCard.directorCreditCost;
                    bossBody.GetComponent<SummonerBossBodyBehavior>().ghostBodies.Add(spawnedBody);

                    if (bossType == SummonerBossType.SlammerGhost)
                    {
                        // Make the parent ghost's teleport skill usable regardless of health
                        // fraction - it needs it for mobility and it will never use it otherwise
                        foreach (var aiSkillDriver in spawnedBody.master.GetSkillDrivers("Teleport"))
                        {
                            aiSkillDriver.maxUserHealthFraction = float.PositiveInfinity;
                        }
                    }
                }
            }
        }

        private static bool CanBeSupportMonster(WeightedSelection<DirectorCard>.ChoiceInfo choice)
        {
            // TODO: check all bosses' interactions with artifact of kin
            return choice.value?.cost > 14 && choice.value?.spawnCard is CharacterSpawnCard characterSpawnCard && characterSpawnCard.prefab.GetComponent<CharacterMaster>()?.bodyPrefab?.GetComponent<CharacterBody>()?.isChampion == false;
        }

        private static bool CanBeMainBossMonster(WeightedSelection<DirectorCard>.ChoiceInfo choice)
        {
            return choice.value.cost > 10;
        }

        private void SummonerBossFightBehavior_OnSpawnedServer(SpawnCard.SpawnResult result)
        {
            if (!Utils.TryGetCharacterBody(result.spawnedInstance, out var body))
            {
                return;
            }

            var summonerBody = this.References.MainBossBodyCurrentlySummoningSupport;

            if (summonerBody is null)
            {
                this.OnMainBossSpawnedServer(result, body!);
            }
            else
            {
                if (EntityStateMachine.TryFindByCustomName(summonerBody.gameObject, StateMachineCustomName, out var stateMachine))
                {
                    (stateMachine.state as SummonerStates.SummonerBaseState)?.OnBossSpawnedServer(result, body!);
                }
            }
        }

        private void OnMainBossSpawnedServer(SpawnCard.SpawnResult result, CharacterBody body)
        {
            var wave = this.GetComponent<InfiniteTowerWaveController>();
            this.GetComponent<CombatDirector>().totalCreditsSpent = wave.totalWaveCredits;
            float cost = body.cost;

            if (cost <= 0)
            {
                Debug.LogWarning("CharacterBody.cost is not set for the boss! Defaulting to spawn card cost for health scaling, but this may cause issues if the spawn card cost is not representative of the boss's strength.");
                cost = result.spawnRequest.spawnCard.directorCreditCost;
            }

            float healthMultiplier = wave.totalWaveCredits / cost;

            if (healthMultiplier > 1)
            {
                healthMultiplier = 2800 / (body.baseMaxHealth + body.baseMaxShield);
            }

            Debug.Log($"Scaling health for {body.name} by {healthMultiplier}");
            var summonerBehavior = this.GetComponent<SummonerBossFightBehavior>();
            body.ScaleMaxHealth(summonerBehavior, healthMultiplier);
            Utils.MakeBodySemiImmortal(body);
            body.DisableStunsEtc();
            body.EnsureComponent<SummonerBossBodyBehavior>();
            EntityStateMachine bossBodyStateMachine = body.gameObject.AddComponent<EntityStateMachine>();
            bossBodyStateMachine.customName = StateMachineCustomName;
            body.healthComponent.ForwardBossDamageTo(bossBodyStateMachine);
            this.SpawnGhosts(body, SummonerBossPowerLevel.Phase1);
            bossBodyStateMachine.SetState(new SummonerStates.Phase1 { References = this.References });
        }

        private DirectorCard[] GetSupportMonsterDirectorCards(int count) // TODO: if the main boss is a lunar chimera wisp, for some reason this can return wisp and golem rather than exploder and golem.
        {
            var selector = this.combatDirector!.finalMonsterCardsSelection;
            var list = new List<DirectorCard>();
            var ignored = new int[] { this.References.MainBossMonsterIndex }.Union(Enumerable.Range(0, selector.Count).Where(x => !CanBeSupportMonster(selector.GetChoice(x)))).ToArray();

            for (int i = 0; i < count; i++)
            {
                var choiceIndex = selector.EvaluateToChoiceIndex(this.combatDirector.rng.nextNormalizedFloat, ignored);
                list.Add(selector.GetChoice(choiceIndex).value);
                ArrayUtils.ArrayAppend(ref ignored, choiceIndex);
            }

            return list.ToArray();
        }

        private int GetMainBossMonsterIndex()
        {
            var selector = this.combatDirector!.finalMonsterCardsSelection;
            var ignored = Enumerable.Range(0, selector.Count).Where(x => !CanBeMainBossMonster(selector.GetChoice(x))).ToArray();
            return selector.EvaluateToChoiceIndex(this.combatDirector.rng.nextNormalizedFloat, ignored);
        }
    }
}