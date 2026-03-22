using PactOfPunishment.Conditions;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage4
{
    public class Gup : Stage4MiniBossWaveDefinition // TODO: add BrotherHauntBody, and some kind of projectiles to the other stage 4 mini-bosses other than Aurelionite?
    {
        protected override uint BaseMaxSquadCount => 2;

        protected override float AddSpawnInterval => 15;

        protected override DirectorCardCategorySelection GetAddsMonsterCards(Stage4AddsSpawnCards cards)
        {
            return Utils.MakeDirectorCardCategorySelection(
                ("Invalidator", new[] { cards.invalidatorSpawnCard })
            );
        }

        protected override AssetPromise<CharacterSpawnCard> GetBossSpawnCard(Stage4MiniBossSpawnCards cards)
        {
            return cards.gupSpawnCard;
        }

        protected override UpgradeEncounterStrategy? GetUpgradeStrategy()
        {
            var output = ScriptableObject.CreateInstance<AddExtraMiniBossUpgradeStrategy>();
            output.extraBossSpawnInfo = new InfiniteTowerExplicitSpawnWaveController.SpawnInfo
            {
                count = 1,
                spawnCard = Stage4MiniBossSpawnCards.Instance.invalidatorSpawnCard.Value,
            };
            output.spawnDelay = 5;
            return output;
        }

        public class BodyBehavior : MonoBehaviour
        {
            public void Awake()
            {
                var body = this.GetComponent<CharacterBody>();
                Utils.MakeUnscaledEliteUsingEquipment(body, RoR2Content.Elites.Fire);
                body.inventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, 10); // Use items so they are passed to Geeps and Gips.
            }
        }
    }
}