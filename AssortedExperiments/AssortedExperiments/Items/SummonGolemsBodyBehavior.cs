using HG;
using RoR2;
using RoR2.Items;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace AssortedExperiments.Items
{
    public class SummonGolemsTargetBehavior : MonoBehaviour, CombatDirector.ICustomPlacementBase
    {
        public void GetSpawnTarget(Transform currentSpawnTarget, out Transform target, out DirectorPlacementRule.PlacementMode spawnMode)
        {
            target = this.transform; // TODO: this is correct, right?
            spawnMode = DirectorPlacementRule.PlacementMode.Approximate;
        }
    }

    public class SummonGolemsBodyBehavior : BaseItemBodyBehavior
    {
        private CombatDirector? combatDirector;

        internal static GameObject? CombatDirectorPrefab { get; set; }

        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return Content.Items.SummonGolems;
        }

        public static void EliminateSquad(CombatSquad squad)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            while (squad.memberCount > 0)
            {
                var memberMaster = squad.readOnlyMembersList[0];
                var healthComponent = memberMaster.Then(x => x.GetBody()).Then(x => x.healthComponent);

                if (healthComponent)
                {
                    healthComponent!.Suicide();
                }
            }
        }

        public override void OnInventoryRefresh()
        {
            base.OnInventoryRefresh();
            this.ScaleCombatDirector(this.combatDirector!);
        }

        internal static void SetupCombatDirectorPrefab(CombatDirector x)
        {
            x.enabled = false;
            x.combatSquad = x.gameObject.AddComponent<CombatSquad>();
            x.combatSquad.grantBonusHealthInMultiplayer = false;

            x.eliteBias = 2;
            x.expRewardCoefficient = 0;
            x.fallBackToStageMonsterCards = false;
            x.goldRewardCoefficient = 0;
            x.maxRerollSpawnInterval = 15;
            x.maxSquadCount = 5;
            x.minRerollSpawnInterval = 15;
            x.moneyWaveIntervals = new RangeFloat[] { new RangeFloat { min = 1, max = 1 } }; // TODO: look more into this and how CombatDirectors use difficulty coefficient etc.
            x.onSpawnedServer = new CombatDirector.OnSpawnedServer();

            // TODO: see if rng needs to be based on something other than stageRng (probably not)
            x.skipSpawnIfTooCheap = false;

            // TODO: x.spawnEffectPrefab? Probably don't need to set this
            x.targetPlayers = false;

            x.customPlacementBase = x.gameObject.AddComponent<SummonGolemsTargetBehavior>();
            var monsterCards = ScriptableObject.CreateInstance<DirectorCardCategorySelection>();
            monsterCards.categories = new DirectorCardCategorySelection.Category[]
            {
                new DirectorCardCategorySelection.Category
                {
                    name = "Golems Only",
                    selectionWeight = 1,
                    cards = new DirectorCard[] {
                        new DirectorCard
                        {
                            spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Golem/cscGolem.asset").WaitForCompletion(), // TODO: is this the correct way of doing this?
                            selectionWeight = 25
                        }
                    }
                }
            };
            x.monsterCards = monsterCards;
        }

        private static void SetTeam(CombatDirector combatDirector, TeamIndex teamIndex)
        {
            combatDirector.teamIndex = teamIndex;

            var membersToRemove = combatDirector.combatSquad.readOnlyMembersList.Where(x => x.teamIndex != teamIndex).ToList();

            foreach (var member in membersToRemove)
            {
                combatDirector.combatSquad.RemoveMember(member);
            }
        }

        private static Action<TeamComponent, TeamIndex> TeamComponent_onJoinTeamGlobal(CharacterBody body, CombatDirector combatDirector) => (arg1, arg2) =>
        {
            if (arg1 == body.teamComponent)
            {
                SetTeam(combatDirector, arg2);
            }
        };

        private void OnEnable()
        {
            var anchor = this.gameObject.EnsureComponent<CombatDirectorAnchor>();
            bool isFirstInit = false;

            if (!anchor.CombatDirector)
            {
                isFirstInit = true;
                anchor.Init(CombatDirectorPrefab);
            }

            this.combatDirector = anchor.CombatDirector;
            SetTeam(this.combatDirector!, this.body.master.teamIndex);

            if (isFirstInit)
            {
                this.combatDirector!.SetMonsterCredit(200 * this.combatDirector.creditMultiplier);
                anchor.SetJoinTeamListener(TeamComponent_onJoinTeamGlobal(this.body, this.combatDirector));
            }

            this.combatDirector!.onSpawnedServer.AddListener(this.OnGolemSpawned); // what if combat director is null?
            this.combatDirector.enabled = true;
        }

        private void OnDisable()
        {
            this.combatDirector!.enabled = false;
            this.combatDirector.onSpawnedServer.RemoveListener(this.OnGolemSpawned);
        }

        private void ScaleCombatDirector(CombatDirector x)
        {
            if (NetworkServer.active)
            {
                x.moneyWaves[0].multiplier = x.creditMultiplier = this.stack;
            }
        }

        private void OnGolemSpawned(GameObject spawnedEntity)
        {
            Inventory component = spawnedEntity.GetComponent<Inventory>();

            if (component)
            {
                component.GiveItemPermanent(Content.Items.InstantDeathMark, 3);
            }
            else
            {
                Debug.LogWarning("SummonGolemsBodyBehavior: spawned entity has no Inventory component to give InstantDeathMark item to.");
            }

            spawnedEntity.GetComponent<CharacterMaster>().minionOwnership.SetOwner(this.body.master);
        }
    }
}