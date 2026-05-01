using PactOfPunishment.AiSkillDrivers;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage4
{
    public class BlazingElderLemurian : Stage4MiniBossWaveDefinition
    {
        protected override uint BaseMaxSquadCount => 0; // maybe pick something?

        protected override float AddSpawnInterval => 1; // TODO: very low, considering

        protected override DirectorCardCategorySelection GetAddsMonsterCards(Stage4AddsSpawnCards cards)
        {
            return Utils.MakeDirectorCardCategorySelection(("Larva", new[] { cards.larvaSpawnCard.Value }));
        }

        protected override AssetPromise<CharacterSpawnCard> GetBossSpawnCard(Stage4MiniBossSpawnCards cards)
        {
            return cards.elderLemurianSpawnCard;
        }

        protected override UpgradeEncounterStrategy? GetUpgradeStrategy()
        {
            var output = ScriptableObject.CreateInstance<AddExtraMiniBossUpgradeStrategy>();
            output.extraBossSpawnInfo = new InfiniteTowerExplicitSpawnWaveController.SpawnInfo
            {
                count = 1,
                spawnCard = Stage4MiniBossSpawnCards.Instance.gupSpawnCard.Value,
            };
            output.spawnDelay = 3;
            return output;
        }

        public class BodyBehavior : BossBodyBehavior
        {
            public void OnEnable()
            {
                RecalculateStats.Add(this.Body, OnRecalculateStats);
            }

            public void OnDisable()
            {
                RecalculateStats.Remove(this.Body, OnRecalculateStats);
            }

            protected override void Awake()
            {
                base.Awake();
                Utils.MakeUnscaledEliteUsingEquipment(this.Body, RoR2Content.Elites.Fire);
                this.Body.inventory.GiveItemPermanent(RoR2Content.Items.JumpBoost, 8);

                foreach (var skillDriver in this.Body.GetSkillDrivers("ChaseTarget"))
                {
                    skillDriver.shouldSprint = true;
                }
            }

            private static void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.healthTotalMult *= 2.5f;
                args.damageTotalMult *= 1.5f;
                args.jumpPowerTotalMult /= 30f;

                // TODO is this enough?
            }
        }
    }
}