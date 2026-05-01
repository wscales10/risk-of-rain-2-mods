using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage4
{
    public class Aurelionite : Stage4MiniBossWaveDefinition
    {
        protected override uint BaseMaxSquadCount => 4;

        protected override float AddSpawnInterval => 6;

        protected override DirectorCardCategorySelection GetAddsMonsterCards(Stage4AddsSpawnCards cards) => Utils.MakeDirectorCardCategorySelection(
            ("Ranged", new[] { cards.lemurianSpawnCard.Value }),
            ("Melee", new[] { cards.geepSpawnCard.Value })
        );

        protected override AssetPromise<CharacterSpawnCard> GetBossSpawnCard(Stage4MiniBossSpawnCards cards)
        {
            return cards.aurelioniteSpawnCard;
        }

        protected override PickupDropTable GetRewardDropTable(Run run)
        {
            return BetterExplicitPickupDropTable.ReplaceTierWithSingleItem(GetBaseDropTable(run), RoR2Content.Items.TitanGoldDuringTP); 
        }

        protected override UpgradeEncounterStrategy? GetUpgradeStrategy()
        {
            var output = ScriptableObject.CreateInstance<AddExtraMiniBossUpgradeStrategy>();
            output.extraBossSpawnInfo = new InfiniteTowerExplicitSpawnWaveController.SpawnInfo
            {
                count = 1,
                spawnCard = Stage4MiniBossSpawnCards.Instance.gupSpawnCard.Value,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Close,
            };
            output.spawnDelay = 8;
            return output;
        }

        public class BodyBehavior : BossBodyBehavior
        {
            protected override void Awake()
            {
                base.Awake();
                this.Body.inventory.GiveItemPermanent(RoR2Content.Items.AdaptiveArmor);
            }

            public void OnEnable()
            {
                RecalculateStats.Add(this.Body, this.OnRecalculateStats);
            }

            public void OnDisable()
            {
                RecalculateStats.Remove(this.Body, this.OnRecalculateStats);
            }

            private void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                // TODO
            }
        }
    }
}