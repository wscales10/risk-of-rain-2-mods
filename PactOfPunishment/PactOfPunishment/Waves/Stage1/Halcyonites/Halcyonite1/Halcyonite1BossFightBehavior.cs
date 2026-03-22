using HG;
using RoR2;
using RoR2.CharacterAI;
using System;

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

        protected override void OnBossSpawnedServer(CharacterBody body)
        {
            if (body.Is(DLC2Content.BodyPrefabs.HalcyoniteBody))
            {
                body.ScaleDifficultyAsBoss(0.65f, 65f, true, false); // TODO: rethink the way I'm scaling enemies, I need one or more helper methods which easily allow me to correctly scale enemy health, damage and most importantly, rewards. Also note that the combat squads scale enemy health for multiplayer by default, so at the moment I'm overscaling.
                this.laserFirst ??= this.GetComponent<CombatDirector>().rng.nextBool;

                this.SetupBossAi(body);

                var halcyoniteBodyBehavior = body.EnsureComponent<Halcyonite1BodyBehavior>();
                halcyoniteBodyBehavior.laserFirst = this.laserFirst.Value;
                halcyoniteBodyBehavior.CombatDirector = this.CombatDirector;
                body.inventory.GiveItemPermanent(RoR2Content.Items.SecondarySkillMagazine, 2);
                body.DisableStunsEtc();
            }
            else if (body.Is(RoR2Content.BodyPrefabs.GolemBody))
            {
                body.ScaleDifficultyAsBoss(4.5f, 30f, true, false);
                this.DisableSkill(body, SkillSlot.Secondary);
            }
            else if (body.Is(DLC2Content.BodyPrefabs.ChildBody))
            {
                body.ScaleDifficultyAsBoss(4.5f, 65f, true, false);
                body.EnsureComponent<DisableChildMonsterTeleport>();
            }
        }

        protected override void SetupBossAi(BaseAI ai)
        {
            base.SetupBossAi(ai);

            ai.aimVectorMaxSpeed = 720f; // Turn twice as fast

            int index = Array.FindIndex(ai.skillDrivers, x => x.customName == "WhirlwindRush");

            if (index != -1)
            {
                CustomWeaponStates.CrossedFistsSkillState.customSkill.InsertSkillDriver(ai, index);
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