using PactOfPunishment.Conditions;
using R2API;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage4
{
    public class Invalidator : Stage4MiniBossWaveDefinition
    {

        protected override uint BaseMaxSquadCount => 3;

        protected override float AddSpawnInterval => 3;

        protected override DirectorCardCategorySelection GetAddsMonsterCards(Stage4AddsSpawnCards cards)
        {
            return Utils.MakeDirectorCardCategorySelection(("Melee", new[] { cards.geepSpawnCard.Value }));
        }

        protected override AssetPromise<CharacterSpawnCard> GetBossSpawnCard(Stage4MiniBossSpawnCards cards)
        {
            return cards.invalidatorSpawnCard;
        }

        protected override UpgradeEncounterStrategy? GetUpgradeStrategy()
        {
            var output = ScriptableObject.CreateInstance<AddExtraMiniBossUpgradeStrategy>();
            output.extraBossSpawnInfo = new InfiniteTowerExplicitSpawnWaveController.SpawnInfo
            {
                count = 1,
                spawnCard = Stage4MiniBossSpawnCards.Instance.gupSpawnCard.Value,
            };
            output.spawnDelay = 6;
            return output;
        }

        public class BodyBehavior : MonoBehaviour
        {
            // TODO do I need a visual effect?
            public void OnEnable()
            {
                RecalculateStats.Add(this.GetComponent<CharacterBody>(), this.OnRecalculateStats);
            }

            public void OnDisable()
            {
                RecalculateStats.Remove(this.GetComponent<CharacterBody>(), this.OnRecalculateStats);
            }

            private void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.damageTotalMult *= 2; // TODO: check this - I could probably raise it higher. Will also need to add other dangers to these mini boss waves.
                args.moveSpeedTotalMult *= 1.3f;
                args.healthTotalMult *= 5.5f;
            }
        }
    }
}