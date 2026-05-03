using HG;
using RoR2;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1
{
    public class Halcyonite1BossFightBehavior : Stage1HalcyoniteBossFightBehavior
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

            this.gameObject.EliminateCombatSquadWhenLastMainMemberDies(this.CombatDirector.combatSquad, x => x.GetBody()?.bodyIndex == DLC2Content.BodyPrefabs.HalcyoniteBody.bodyIndex, callback: () => this.CombatDirector.enabled = false);
        }

        protected override void OnMainBossSpawnedServer(CharacterBody body)
        {
            this.laserFirst ??= this.GetComponent<CombatDirector>().rng.nextBool;

            var halcyoniteBodyBehavior = body.EnsureComponent<Halcyonite1BodyBehavior>();
            halcyoniteBodyBehavior.laserFirst = this.laserFirst.Value;
            halcyoniteBodyBehavior.CombatDirector = this.CombatDirector;
            halcyoniteBodyBehavior.BossStateMachine.SetState(new Halcyonite1States.Phase1());
        }

        protected override void OnAddSpawnedServer(CharacterBody body)
        {
            base.OnAddSpawnedServer(body);

            if (body.Is(RoR2Content.BodyPrefabs.GolemBody))
            {
                body.ScaleDifficultyAsBoss(new BossScalingArgs1(65f, 30f, false, 10), false);
                body.ScaleMaxHealth(this, 0.4f);
                body.ScaleDamage(this, 0.85f);
                this.DisableSkill(body, SkillSlot.Secondary);
            }
            else if (body.Is(DLC2Content.BodyPrefabs.ChildBody))
            {
                body.ScaleDifficultyAsBoss(new BossScalingArgs1(65f, 65f, false, 10), false);
                body.ScaleMaxHealth(this, 0.6f);
                body.ScaleDamage(this, 0.25f);
                body.EnsureComponent<DisableChildMonsterTeleport>();
            }
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
    }
}