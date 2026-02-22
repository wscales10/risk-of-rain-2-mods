using HG;
using PactOfPunishment.Conditions;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage2.Summoner
{
    public class SummonerBossFightBehavior : MonoBehaviour
    {
        public const string StateMachineCustomName = "SummonerBossBody";

        public SummonerReferences References;

        private CombatDirector? combatDirector;

        public void Awake()
        {
            this.combatDirector = this.GetComponent<CombatDirector>();
            this.combatDirector.EnsureComponent<UseMinimumEliteTierBehavior>();
            MonsterTracker.TrackCombatDirector(this.combatDirector);

            var selector = this.combatDirector.finalMonsterCardsSelection;

            this.References = this.EnsureComponent<SummonerReferences>();
            this.References.MainBossMonsterIndex = selector.EvaluateToChoiceIndex(this.combatDirector.rng.nextNormalizedFloat);
            this.References.SupportMonsterDirectorCards = this.GetSupportMonsterDirectorCards(2);

            var mainBossMonsterDirectorCard = selector.GetChoice(this.References.MainBossMonsterIndex).value;
            Debug.Log($"Selected summoner boss: '{mainBossMonsterDirectorCard?.spawnCard.prefab.name}'");
            this.combatDirector.OverrideNextBossCard(mainBossMonsterDirectorCard, false); // TODO: can fail! Try on commencement?
        }

        public void OnEnable()
        {
            SpawnCard.onSpawnedServerGlobal += this.SpawnCard_onSpawnedServerGlobal;
        }

        public void OnDisable()
        {
            SpawnCard.onSpawnedServerGlobal -= this.SpawnCard_onSpawnedServerGlobal;
        }

        private static bool CanBeSupportMonster(WeightedSelection<DirectorCard>.ChoiceInfo choice)
        {
            // TODO: check all bosses' interactions with artifact of kin
            return choice.value?.cost > 14 && choice.value?.spawnCard is CharacterSpawnCard characterSpawnCard && characterSpawnCard.prefab.GetComponent<CharacterMaster>()?.bodyPrefab?.GetComponent<CharacterBody>()?.isChampion == false;
        }

        private void SpawnCard_onSpawnedServerGlobal(SpawnCard.SpawnResult result)
        {
            if (!result.success || !result.spawnedInstance || !result.spawnedInstance.TryGetComponent<MonsterTracker>(out var tracker) || tracker.combatDirector != this.combatDirector)
            {
                return;
            }

            var body = Utils.GetCharacterBody(result.spawnedInstance);

            if (!body)
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
            float healthMultiplier = wave.totalWaveCredits / result.spawnRequest.spawnCard.directorCreditCost;
            Debug.Log($"Scaling health for {body.name} by {healthMultiplier}");
            var summonerBehavior = this.GetComponent<SummonerBossFightBehavior>();
            body.ScaleMaxHealth(summonerBehavior, healthMultiplier);
            Utils.MakeBodySemiImmortal(body);
            body.DisableStunsEtc();
            EntityStateMachine bossBodyStateMachine = body.gameObject.AddComponent<EntityStateMachine>();
            bossBodyStateMachine.customName = StateMachineCustomName;
            body.healthComponent.ForwardBossDamageTo(bossBodyStateMachine);
            bossBodyStateMachine.SetState(new SummonerStates.Phase1 { References = this.References });
        }

        private DirectorCard[] GetSupportMonsterDirectorCards(int count)
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
    }
}