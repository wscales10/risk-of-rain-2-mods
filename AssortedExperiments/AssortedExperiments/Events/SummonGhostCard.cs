using RoR2;
using System;

namespace AssortedExperiments.Events
{
    public class SummonGhostCard
    {
        private CharacterSpawnCard? spawnCard;
        private DirectorCard? directorCard;

        public SummonGhostCard(string spawnCardName, PlacementRuleGetter getPlacementRule, params Func<SkillLocator, GenericSkill>[] banSkills)
        {
            this.SpawnCardName = spawnCardName;
            this.GetPlacementRule = getPlacementRule;
            this.BanSkills = banSkills;
        }

        public string SpawnCardName { get; }

        public CharacterSpawnCard? SpawnCard
        {
            get => this.spawnCard;

            set
            {
                this.spawnCard = value;
                this.directorCard = new DirectorCard
                {
                    spawnCard = this.spawnCard,
                };
                this.IsChampion = value?.prefab.GetComponent<CharacterMaster>()?.bodyPrefab.GetComponent<CharacterBody>()?.isChampion;
            }
        }

        public bool IsAvailable => this.directorCard?.IsAvailable() == true;

        public float StartingHealthFraction { get; set; } = 1;

        public int Lifespan { get; set; } = 8;

        public PlacementRuleGetter GetPlacementRule { get; }

        public Func<SkillLocator, GenericSkill>[] BanSkills { get; }

        public bool CanDoFriendlyFire { get; set; } = false;

        public bool? IsChampion { get; private set; }
    }
}